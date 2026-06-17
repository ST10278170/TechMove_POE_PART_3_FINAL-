using Microsoft.AspNetCore.Mvc;
using TechMove.GLMS.API.DTOs;
using TechMove.GLMS.API.Services;

namespace TechMove.GLMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequestDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            if (result == null)
                return Unauthorized(new { message = "Invalid username or password" });

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            if (!result)
                return BadRequest(new { message = "User already exists" });

            return Created("", new { message = "User registered successfully" });
        }
    }
}
