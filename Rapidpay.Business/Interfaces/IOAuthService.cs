using Rapidpay.Data.Models;

namespace Rapidpay.Business.Interfaces;

public interface IOAuthService
{
    Task<OAuthClient> RegisterClientAsync(string clientId, string clientSecret, string redirectUri, string[] scopes);
    Task<string> GenerateAuthorizationCodeAsync(OAuthAuthorizationRequest request, int userId);
    Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(OAuthTokenRequest request);
    Task<(string AccessToken, string RefreshToken)> RefreshTokensAsync(string refreshToken);
    Task<bool> ValidateAccessTokenAsync(string accessToken);
    Task<bool> RevokeTokenAsync(string token);
    Task<OAuthClient?> GetClientAsync(string clientId);
    Task<bool> ValidateClientCredentialsAsync(string clientId, string clientSecret);
} 