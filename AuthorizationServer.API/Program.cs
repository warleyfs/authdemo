using AuthorizationServer.API.Data;
using AuthorizationServer.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthorizationServer.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers()
            .AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = null; });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Configure Entity Framework Core with SQL Server using the connection string from configuration
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("EFCoreDBConnection")));
        
        // Register the KeyRotationService as a hosted (background) service
        // This service handles periodic rotation of signing keys to enhance security
        builder.Services.AddHostedService<KeyRotationService>();
        
        // Configure Authentication using JWT Bearer tokens
        builder.Services.AddAuthentication(options =>
        {
            // This indicates the authentication scheme that will be used by default when the app attempts to authenticate a user.
            // Which authentication handler to use for verifying who the user is by default.
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            
            // This indicates the authentication scheme that will be used by default when the app encounters an authentication challenge. 
            // Which authentication handler to use for responding to failed authentication or authorization attempts.
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            // Define token validation parameters to ensure tokens are valid and trustworthy
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true, // Ensure the token was issued by a trusted issuer
                ValidIssuer = builder.Configuration["Jwt:Issuer"], // The expected issuer value from configuration
                ValidateAudience = false, // Disable audience validation (can be enabled as needed)
                ValidateLifetime = true, // Ensure the token has not expired
                ValidateIssuerSigningKey = true, // Ensure the token's signing key is valid
                
                // Define a custom IssuerSigningKeyResolver to dynamically retrieve signing keys from the JWKS endpoint
                IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                {
                    //Console.WriteLine($"Received Token: {token}");
                    //Console.WriteLine($"Token Issuer: {securityToken.Issuer}");
                    //Console.WriteLine($"Key ID: {kid}");
                    //Console.WriteLine($"Validate Lifetime: {parameters.ValidateLifetime}");
                    // Initialize an HttpClient instance for fetching the JWKS
                    var httpClient = new HttpClient();
                    
                    // Synchronously fetch the JWKS (JSON Web Key Set) from the specified URL
                    var jwks = httpClient
                        .GetStringAsync($"{builder.Configuration["Jwt:Issuer"]}/.well-known/jwks.json").Result;
                    
                    // Parse the fetched JWKS into a JsonWebKeySet object
                    var keys = new JsonWebKeySet(jwks);
                    
                    // Return the collection of JsonWebKey objects for token validation
                    return keys.Keys;
                }
            };
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}