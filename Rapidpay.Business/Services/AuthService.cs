namespace Rapidpay.Business.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Rapidpay.Data.Models;
using Rapidpay.Business.Interfaces;
using Rapidpay.Data.Interfaces;

public class AuthService : IAuthService
{   
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<AuthResponse> Register(User user)
    {
        var existingUser = await _unitOfWork.UserRepository.FindAsync(user.Id);
        if (existingUser != null)
            throw new InvalidOperationException("User already exists");

        var newUser = new User
        {
            Username = user.Username,
            Email = user.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash),
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        var response = new AuthResponse
        {
            Username = user.Username,
            Token = GenerateJwtToken(user.Username),
            RefreshToken = GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpirationMinutes"]))
        };
        await _unitOfWork.UserRepository.AddAsync(newUser);
        await _unitOfWork.SaveAsync();

        return response;
    }
    public async Task<AuthResponse?> Login(LoginRequest request)
    {
        var user = await _unitOfWork.UserRepository.GetAllAsync(u => u.Username == request.Username);
        if (user == null)
            throw new InvalidOperationException("User not found");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.First().PasswordHash))
            throw new InvalidOperationException("Invalid password");

        return GenerateAuthResponse(user.First());
    }

    public async Task<AuthResponse?> RefreshToken(string refreshToken)
    {
        var user = await _unitOfWork.UserRepository.GetAllAsync(u => u.RefreshToken == refreshToken);
        if (user == null)
            throw new InvalidOperationException("User not found");

        if (user.First().RefreshTokenExpiry < DateTime.UtcNow)
            throw new InvalidOperationException("Refresh token expired");

        return GenerateAuthResponse(user.First());
    }

    private async Task<AuthResponse> GenerateAuthResponse(User user)
    {
        var authResponse = new AuthResponse
        {
            Username = user.Username,
            Token = GenerateJwtToken(user.Username),
            RefreshToken = GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpirationMinutes"]))
        };
        return authResponse;
    }
    private string GenerateJwtToken(string username)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(secretKey);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpirationMinutes"]));

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }

}
