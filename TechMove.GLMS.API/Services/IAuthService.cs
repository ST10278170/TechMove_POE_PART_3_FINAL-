using TechMove.GLMS.API.DTOs;

namespace TechMove.GLMS.API.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(AuthRequestDto dto);
        Task<bool> RegisterAsync(RegisterDto dto);
    }
}
