using Microsoft.AspNetCore.Mvc;
using JWT.RequestModels;
using JWT.Services;

namespace JWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {

        [HttpPost("login")]
        public async Task<IActionResult> Login(RequestModels.LoginRequestModel loginRequestModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await authService.LoginAsync(loginRequestModel);
            if (response != null)
            {
                return Ok(response);
            }
            return Unauthorized("Wrong username or password");
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequestModel refreshTokenRequestModel)
        {
            var response = await authService.RefreshTokenAsync(refreshTokenRequestModel.RefreshToken);
            if (response != null)
            {
                return Ok(response);
            }
            return Unauthorized("Invalid token");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestModel registerRequestModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await authService.RegisterAsync(registerRequestModel);
            if (success)
            {
                return Ok("User registered successfully");
            }
            return BadRequest("Username already exists");
        }
    }
}