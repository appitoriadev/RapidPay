namespace Rapidpay.Data.Models;

public class OAuthClient
{
    public int Id { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string RedirectUri { get; set; }
    public string[] AllowedScopes { get; set; }
    public bool IsActive { get; set; }
}

public class OAuthAuthorizationCode
{
    public int Id { get; set; }
    public string Code { get; set; }
    public int ClientId { get; set; }
    public int UserId { get; set; }
    public string RedirectUri { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OAuthToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ClientId { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string[] Scopes { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OAuthAuthorizationRequest
{
    public string ResponseType { get; set; }  // "code" for authorization code flow
    public string ClientId { get; set; }
    public string RedirectUri { get; set; }
    public string Scope { get; set; }
    public string State { get; set; }
}

public class OAuthTokenRequest
{
    public string GrantType { get; set; }  // "authorization_code", "refresh_token", "password"
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string Code { get; set; }  // For authorization code flow
    public string RefreshToken { get; set; }  // For refresh token flow
    public string Username { get; set; }  // For password grant
    public string Password { get; set; }  // For password grant
    public string Scope { get; set; }
    public string RedirectUri { get; set; }
} 