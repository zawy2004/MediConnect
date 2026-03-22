using MediConnect.Application.DTOs;

namespace MediConnect.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto> LoginAsync(LoginDto dto);
    Task<AuthResultDto> RegisterAsync(RegisterDto dto);
    Task<AuthResultDto> LoginOrRegisterGoogleAsync(string email, string fullName);
}
