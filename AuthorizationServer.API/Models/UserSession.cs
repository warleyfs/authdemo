using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AuthorizationServer.API.Models;

[Index(nameof(Expires), Name = "IX_Unique_Expires")]
public class UserSession
{
    // Random Session Id as part of composite key
    [Key]
    public Guid Id { get; set; }
    
    // Foreign key referencing User.
    [Required]
    public int UserId { get; set; }
    
    // Navigation property to User.
    public User User { get; set; }
    
    // Foreign key referencing Key.
    [Required]
    public int KeyId { get; set; }
    
    // Navigation property to Key.
    public SigningKey Key { get; set; }
    
    // Foreign key referencing Client.
    [Required]
    public int ClientId { get; set; }
    
    // Navigation property to Client.
    public Client Client { get; set; }
    
    // Generated Token to the session.
    [Required]
    public string Token { get; set; }
    
    // Session Expiration
    public DateTime? Expires { get; set; }
}