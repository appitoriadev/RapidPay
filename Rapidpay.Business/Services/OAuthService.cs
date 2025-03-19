using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Rapidpay.Business.Interfaces;
using Rapidpay.Data;
using Rapidpay.Data.Models;

namespace Rapidpay.Business.Services;

public class OAuthService : IOAuthService
{
    private readonly RapidpayDbContext _context;
    private readonly IConfiguration _configuration;

    public OAuthService(RapidpayDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<OAuthClient> RegisterClientAsync(string clientId, string clientSecret, string redirectUri, string[] scopes)
    {
        var client = new OAuthClient
        {
            ClientId = clientId,
            ClientSecret = HashClientSecret(clientSecret),
            RedirectUri = redirectUri,
            AllowedScopes = scopes,
            IsActive = true
        };

        _context.OAuthClients.Add(client);
        await _context.SaveChangesAsync();
        return client;
    }

    public async Task<string> GenerateAuthorizationCodeAsync(OAuthAuthorizationRequest request, int userId)
    {
        var client = await ValidateClientAsync(request.ClientId, request.RedirectUri);
        if (client == null)
        {
            throw new InvalidOperationException("Invalid client");
        }

        var code = GenerateRandomString(32);
        var authCode = new OAuthAuthorizationCode
        {
            Code = code,
            ClientId = client.Id,
            UserId = userId,
            RedirectUri = request.RedirectUri,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow
        };

        _context.OAuthAuthorizationCodes.Add(authCode);
        await _context.SaveChangesAsync();
        return code;
    }

    public async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(OAuthTokenRequest request)
    {
        var client = await ValidateClientAsync(request.ClientId, request.RedirectUri);
        if (client == null)
        {
            throw new InvalidOperationException("Invalid client");
        }

        int userId;
        switch (request.GrantType)
        {
            case "authorization_code":
                var authCode = await _context.OAuthAuthorizationCodes
                    .FirstOrDefaultAsync(c => c.Code == request.Code && c.ExpiresAt > DateTime.UtcNow);
                if (authCode == null)
                {
                    throw new InvalidOperationException("Invalid or expired authorization code");
                }
                userId = authCode.UserId;
                break;

            case "password":
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == request.Username);
                if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
                {
                    throw new InvalidOperationException("Invalid username or password");
                }
                userId = user.Id;
                break;

            default:
                throw new InvalidOperationException("Unsupported grant type");
        }

        var accessToken = GenerateAccessToken(userId, client.Id, request.Scope?.Split(' '));
        var refreshToken = GenerateRefreshToken();

        var token = new OAuthToken
        {
            UserId = userId,
            ClientId = client.Id,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Scopes = request.Scope?.Split(' ') ?? Array.Empty<string>(),
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow
        };

        _context.OAuthTokens.Add(token);
        await _context.SaveChangesAsync();

        return (accessToken, refreshToken);
    }

    public async Task<(string AccessToken, string RefreshToken)> RefreshTokensAsync(string refreshToken)
    {
        var token = await _context.OAuthTokens
            .FirstOrDefaultAsync(t => t.RefreshToken == refreshToken);

        if (token == null)
        {
            throw new InvalidOperationException("Invalid refresh token");
        }

        var newAccessToken = GenerateAccessToken(token.UserId, token.ClientId, token.Scopes);
        var newRefreshToken = GenerateRefreshToken();

        token.AccessToken = newAccessToken;
        token.RefreshToken = newRefreshToken;
        token.ExpiresAt = DateTime.UtcNow.AddHours(1);
        token.CreatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (newAccessToken, newRefreshToken);
    }

    public async Task<bool> ValidateAccessTokenAsync(string accessToken)
    {
        var token = await _context.OAuthTokens
            .FirstOrDefaultAsync(t => t.AccessToken == accessToken && t.ExpiresAt > DateTime.UtcNow);

        return token != null;
    }

    public async Task<bool> RevokeTokenAsync(string token)
    {
        var oauthToken = await _context.OAuthTokens
            .FirstOrDefaultAsync(t => t.AccessToken == token || t.RefreshToken == token);

        if (oauthToken == null)
        {
            return false;
        }

        _context.OAuthTokens.Remove(oauthToken);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<OAuthClient?> GetClientAsync(string clientId)
    {
        return await _context.OAuthClients
            .FirstOrDefaultAsync(c => c.ClientId == clientId && c.IsActive);
    }

    public async Task<bool> ValidateClientCredentialsAsync(string clientId, string clientSecret)
    {
        var client = await GetClientAsync(clientId);
        if (client == null)
        {
            return false;
        }

        return VerifyClientSecret(clientSecret, client.ClientSecret);
    }

    private async Task<OAuthClient?> ValidateClientAsync(string clientId, string redirectUri)
    {
        var client = await GetClientAsync(clientId);
        if (client == null || client.RedirectUri != redirectUri)
        {
            return null;
        }
        return client;
    }

    private string GenerateAccessToken(int userId, int clientId, string[] scopes)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("client_id", clientId.ToString())
        };

        if (scopes != null)
        {
            claims.AddRange(scopes.Select(scope => new Claim("scope", scope)));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        return GenerateRandomString(64);
    }

    private string GenerateRandomString(int length)
    {
        var bytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private string HashClientSecret(string clientSecret)
    {
        return BCrypt.Net.BCrypt.HashPassword(clientSecret);
    }

    private bool VerifyClientSecret(string clientSecret, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(clientSecret, hash);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
} 