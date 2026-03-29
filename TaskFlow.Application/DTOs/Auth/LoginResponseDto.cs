namespace TaskFlow.Application.DTOs.Auth;

public class LoginResponseDto
{
    public string Token { get; set; } = default!;
    public DateTime ExpireAt { get; set; }
}