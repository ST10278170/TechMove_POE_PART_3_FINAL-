using TechMove.GLMS.API.DTOs;
using TechMove.GLMS.API.Helpers;
using TechMove.GLMS.API.Models;
using TechMove.GLMS.API.Repositories;

namespace TechMove.GLMS.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repo;
        private readonly JwtTokenGenerator _tokenGenerator;

        public AuthService(IUserRepository repo, JwtTokenGenerator tokenGenerator)
        {
            _repo = repo;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<AuthResponseDto?> LoginAsync(AuthRequestDto dto)
        {
            var user = await _repo.GetByUsernameAsync(dto.Username);

            if (user == null)
                return null;

            if (user.PasswordHash != dto.Password)
                return null;

            var token = _tokenGenerator.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Username = user.Username,
                Role = user.Role
            };
        }

        public async Task<bool> RegisterAsync(RegisterDto dto)
        {
            var existing = await _repo.GetByUsernameAsync(dto.Username);

            if (existing != null)
                return false;

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = dto.Password,
                Role = dto.Role
            };

            await _repo.AddAsync(user);
            await _repo.SaveChangesAsync();

            return true;
        }
    }
}
