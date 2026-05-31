using Application.Auth.DTOs;
using Application.Auth.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly GoogleSignInUseCase _googleSignInUseCase;

    public AuthController(GoogleSignInUseCase googleSignInUseCase)
    {
        _googleSignInUseCase = googleSignInUseCase;
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleSignIn([FromBody] GoogleAuthRequest request)
    {
        var result = await _googleSignInUseCase.ExecuteAsync(request);
        if (result is null)
            return Unauthorized("Invalid Google token.");

        return Ok(result);
    }
}
