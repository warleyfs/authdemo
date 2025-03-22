﻿using System.ComponentModel.DataAnnotations;

namespace AuthorizationServer.API.DTOs;

public class RegisterDTO
{
    [Required(ErrorMessage = "First name is required.")]
    [MaxLength(50, ErrorMessage = "First name must be less than or equal to 50 characters.")]
    public string Firstname { get; set; }
    
    [MaxLength(50, ErrorMessage = "Last name must be less than or equal to 50 characters.")]
    public string Lastname { get; set; }
    
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [MaxLength(100, ErrorMessage = "Email must be less than or equal to 100 characters.")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    [MaxLength(100, ErrorMessage = "Password must be less than or equal to 100 characters.")]
    public string Password { get; set; }
}