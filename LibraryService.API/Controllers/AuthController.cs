using LibraryService.Application.DTO;
using LibraryService.Application.Services;
using LibraryService.Domain.Entities;
using LibraryService.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.API.Controllers;

public record TokenResponse(string token);

[ApiController]
public class AuthController : Controller
{
    private readonly IAuthenticationService authenticationService;
    
    private readonly JwtSettings jwtSettings;

    public AuthController(IAuthenticationService _authenticationService, JwtSettings _jwtSettings)
    {
        authenticationService = _authenticationService;
        jwtSettings = _jwtSettings;
    }

    [HttpPost("/login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(User user) 
    {
        var validuser = await authenticationService.AuthenticateAsync(user.Email, user.Password);
        if (validuser is null)
            return Unauthorized();


        var token = TokenGenerator.GenerateToken(validuser, jwtSettings);

        return Ok(new TokenResponse(token));
    }

}
