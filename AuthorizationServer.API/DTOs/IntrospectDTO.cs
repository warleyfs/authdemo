using System.ComponentModel.DataAnnotations;

namespace AuthorizationServer.API.DTOs;

public class IntrospectDTO
{
    [Required(ErrorMessage = "Token is required")]
    public string Token { get; set; }
}