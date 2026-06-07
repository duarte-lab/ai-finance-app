using Application.Auth.DTOs;
using Application.Auth.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RegisterUseCase _registerUseCase;
    private readonly LoginUseCase _loginUseCase;
    private readonly RefreshTokenUseCase _refreshTokenUseCase;
    private readonly GoogleSignInUseCase _googleSignInUseCase;

    public AuthController(
        RegisterUseCase registerUseCase,
        LoginUseCase loginUseCase,
        RefreshTokenUseCase refreshTokenUseCase,
        GoogleSignInUseCase googleSignInUseCase)
    {
        _registerUseCase = registerUseCase;
        _loginUseCase = loginUseCase;
        _refreshTokenUseCase = refreshTokenUseCase;
        _googleSignInUseCase = googleSignInUseCase;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _registerUseCase.ExecuteAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _loginUseCase.ExecuteAsync(request);
        if (result is null)
            return Unauthorized("Invalid credentials.");

        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await _refreshTokenUseCase.ExecuteAsync(request);
        if (result is null)
            return Unauthorized("Invalid refresh token.");

        return Ok(result);
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
