﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using AuthorizationServer.API.Data;
using AuthorizationServer.API.DTOs;
using AuthorizationServer.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthorizationServer.API.Controllers;

// Private fields to hold the configuration and database context
// Holds configuration settings from appsettings.json or environment variables
// Database context for interacting with the database
[Route("api/[controller]")]
[ApiController]
public class AuthController(IConfiguration configuration, ApplicationDbContext context) : ControllerBase
{
    // Define the Login endpoint that responds to POST requests at 'api/Auth/Login'
    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
    {
        // Validate the incoming model based on data annotations in LoginDTO
        if (!ModelState.IsValid)
        {
            // If the model is invalid, return a 400 Bad Request with validation errors
            return BadRequest(ModelState);
        }

        // Query the Clients table to verify if the provided ClientId exists
        var client = context.Clients
            .FirstOrDefault(c => c.ClientId == loginDto.ClientId);
        
        // If the client does not exist, return a 401 Unauthorized response
        if (client == null)
        {
            return Unauthorized("Invalid client credentials.");
        }

        // Retrieve the user from the Users table by matching the email (case-insensitive)
        // Also include the UserRoles and associated Roles for later use
        var user = await context.Users
            .Include(u => u.UserRoles) // Include the UserRoles navigation property
            .ThenInclude(ur => ur.Role) // Then include the Role within each UserRole
            .FirstOrDefaultAsync(u => u.Email.ToLower() == loginDto.Email.ToLower());
        
        // If the user does not exist, return a 401 Unauthorized response
        if (user == null)
        {
            // For security reasons, avoid specifying whether the client or user was invalid
            return Unauthorized("Invalid credentials.");
        }

        // Verify the provided password against the stored hashed password using BCrypt
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password);
        
        // If the password is invalid, return a 401 Unauthorized response
        if (!isPasswordValid)
        {
            // Again, avoid specifying whether the client or user was invalid
            return Unauthorized("Invalid credentials.");
        }

        // At this point, authentication is successful. Proceed to generate a JWT token.
        var token = GenerateJwtToken(user, client);
        
        // Return the generated token in a 200 OK response
        return Ok(new { Token = token });
    }

    [HttpPost("Introspection")]
    public async Task<IActionResult> Introspect([FromForm] IntrospectDTO introspectDto)
    {
        // Validate the incoming model based on data annotations in LoginDTO
        if (!ModelState.IsValid)
        {
            // If the model is invalid, return a 400 Bad Request with validation errors
            return BadRequest(ModelState);
        }

        var response = IntrospectToken(introspectDto.Token);
        
        return Ok(response);
    }

    // Private method responsible for generating a JWT token for an authenticated user
    private async Task<string> GenerateJwtToken(User user, Client client)
    {
        // Retrieve the active signing key from the SigningKeys table
        var signingKey = context.SigningKeys.FirstOrDefault(k => k.IsActive);
        
        // If no active signing key is found, throw an exception
        if (signingKey == null)
        {
            throw new Exception("No active signing key available.");
        }

        // Convert the Base64-encoded private key string back to a byte array
        var privateKeyBytes = Convert.FromBase64String(signingKey.PrivateKey);
        
        // Create a new RSA instance for cryptographic operations
        var rsa = RSA.Create();
        
        // Import the RSA private key into the RSA instance
        rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
        
        // Create a new RsaSecurityKey using the RSA instance
        var rsaSecurityKey = new RsaSecurityKey(rsa)
        {
            // Assign the Key ID to link the JWT with the correct public key
            KeyId = signingKey.KeyId
        };
        
        // Define the signing credentials using the RSA security key and specifying the algorithm
        var creds = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);
        
        // Initialize a list of claims to include in the JWT
        var claims = new List<Claim>
        {
            // Subject (sub) claim with the user's ID
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            // JWT ID (jti) claim with a unique identifier for the token
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            // Name claim with the user's first name
            new Claim(ClaimTypes.Name, user.Firstname),
            // NameIdentifier claim with the user's email
            new Claim(ClaimTypes.NameIdentifier, user.Email),
            // Email claim with the user's email
            new Claim(ClaimTypes.Email, user.Email)
        };
        
        // Iterate through the user's roles and add each as a Role claim
        foreach (var userRole in user.UserRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
        }

        // Define the JWT token's properties, including issuer, audience, claims, expiration, and signing credentials
        var tokenDescriptor = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"], // The token issuer, typically your application's URL
            audience: client.ClientURL, // The intended recipient of the token, typically the client's URL
            claims: claims, // The list of claims to include in the token
            expires: DateTime.UtcNow.AddHours(1), // Token expiration time set to 1 hour from now
            signingCredentials: creds // The credentials used to sign the token
        );
        
        // Create a JWT token handler to serialize the token
        var tokenHandler = new JwtSecurityTokenHandler();
        
        // Serialize the token to a string
        var token = tokenHandler.WriteToken(tokenDescriptor);
        
        // Save user session
        context.UserSessions.Add(new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            ClientId = client.Id,
            KeyId = signingKey.Id,
            Token = token,
            Expires = DateTime.UtcNow.AddHours(1)
        });
        
        await context.SaveChangesAsync();
        
        // Return the serialized JWT token
        return token;
    }

    // Private method responsible for JWT token instropection as a validation method
    private object IntrospectToken(string token)
    {
        // Retrieve the active signing key from the SigningKeys table
        var signingKey = context.SigningKeys.FirstOrDefault(k => k.IsActive);
        
        // If no active signing key is found, throw an exception
        if (signingKey == null)
        {
            throw new Exception("No active signing key available.");
        }

        // Convert the Base64-encoded private key string back to a byte array
        var privateKeyBytes = Convert.FromBase64String(signingKey.PrivateKey);
        
        // Create a new RSA instance for cryptographic operations
        var rsa = RSA.Create();
        
        // Import the RSA private key into the RSA instance
        rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
        
        // Create a new RsaSecurityKey using the RSA instance
        var rsaSecurityKey = new RsaSecurityKey(rsa)
        {
            // Assign the Key ID to link the JWT with the correct public key
            KeyId = signingKey.KeyId
        };
        
        var tokenHandler = new JwtSecurityTokenHandler();
        tokenHandler.ReadToken(token);
        tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            // Issuer
            ValidateIssuer = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            
            // Audience
            ValidateAudience = false,
            
            // Lifetime
            ValidateLifetime = true,
            
            // Issuer Signing Key
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = rsaSecurityKey
        }, out var validatedToken);

        if (validatedToken is not JwtSecurityToken jwtSecurityToken)
        {
            throw new SecurityTokenException("Invalid token");
        }
        
        return new { active = true };
    }
}