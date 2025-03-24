using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AuthorizationServer.API.Models;

[Index(nameof(Email), Name = "IX_Unique_Email", IsUnique = true)]
public class User
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string Email { get; set; }
    
    [Required]
    public string Firstname { get; set; }
    
    public string? Lastname { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Password { get; set; }
    
    // Navigation property for many-to-many relationship with Role
    public ICollection<UserRole> UserRoles { get; set; } 
    
    // Navigation property for many-to-many relationship with UserSessions
    public ICollection<UserSession> UserSessions { get; set; } 
}