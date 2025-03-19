using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rapidpay.Business.Interfaces;
using Rapidpay.Data.Models;

namespace Rapidpay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OAuthController : ControllerBase
{
    private readonly IOAuthService _oauthService;
    private readonly IAuthService _authService;

    public OAuthController(IOAuthService oauthService, IAuthService authService)
    {
        _oauthService = oauthService;
        _authService = authService;
    }

    [HttpPost("register")]
    [Authorize]
    public async Task<ActionResult<OAuthClient>> RegisterClient([FromBody] OAuthClientRegistrationRequest request)
    {
        try
        {
            var client = await _oauthService.RegisterClientAsync(
                request.ClientId,
                request.ClientSecret,
                request.RedirectUri,
                request.Scopes);

            return Ok(client);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("authorize")]
    public async Task<ActionResult> Authorize([FromQuery] OAuthAuthorizationRequest request)
    {
        try
        {
            // In a real application, this would show a consent screen
            // For this example, we'll assume the user is already authenticated
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            
            var code = await _oauthService.GenerateAuthorizationCodeAsync(request, userId);
            return Redirect($"{request.RedirectUri}?code={code}&state={request.State}");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("token")]
    public async Task<ActionResult<OAuthTokenResponse>> Token([FromBody] OAuthTokenRequest request)
    {
        try
        {
            var (accessToken, refreshToken) = await _oauthService.GenerateTokensAsync(request);
            return Ok(new OAuthTokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenType = "Bearer",
                ExpiresIn = 3600 // 1 hour
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<OAuthTokenResponse>> Refresh([FromBody] OAuthRefreshRequest request)
    {
        try
        {
            var (accessToken, refreshToken) = await _oauthService.RefreshTokensAsync(request.RefreshToken);
            return Ok(new OAuthTokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenType = "Bearer",
                ExpiresIn = 3600 // 1 hour
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<ActionResult> Revoke([FromBody] OAuthRevokeRequest request)
    {
        var result = await _oauthService.RevokeTokenAsync(request.Token);
        if (!result)
        {
            return BadRequest(new { message = "Invalid token" });
        }
        return Ok();
    }
}

public class OAuthClientRegistrationRequest
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string RedirectUri { get; set; }
    public string[] Scopes { get; set; }
}

public class OAuthTokenResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string TokenType { get; set; }
    public int ExpiresIn { get; set; }
}

public class OAuthRefreshRequest
{
    public string RefreshToken { get; set; }
}

public class OAuthRevokeRequest
{
    public string Token { get; set; }
} 