using LibraryService.WebAPI.Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.WebAPI.Features.Auth.Login
{
    [ApiController]
    public class LoginController : Controller
    {
        private readonly LoginHandler loginHandler;
        private readonly JwtSettings jwtSettings;

        public LoginController(LoginHandler _loginHandler, JwtSettings _jwtSettings)
        {
            loginHandler = _loginHandler;
            jwtSettings = _jwtSettings;
        }

        [HttpPost("/login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(User user)
        {
            var validUser = await loginHandler.AuthenticateAsync(user.Email, user.Password);
            if (validUser is null)
                return Unauthorized();

            var token = TokenGenerator.GenerateToken(validUser, jwtSettings);

            return Ok(new TokenResponse(token));
        }
    }
}
