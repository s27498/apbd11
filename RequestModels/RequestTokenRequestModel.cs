namespace JWT.RequestModels;

using System.ComponentModel.DataAnnotations;

public class RefreshTokenRequestModel
{
    [Required] public string RefreshToken { get; set; }
}