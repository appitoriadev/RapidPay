using Rapidpay.Data.Models;

namespace Rapidpay.Business.Interfaces;

public interface IAuthService
{
    // Task<AuthResponse> LoginAsync(LoginRequest request);
    // Task<AuthResponse> RegisterAsync(RegisterRequest request);
    // Task<User?> GetUserByIdAsync(int id);
    // Task<User?> GetUserByUsernameAsync(string username);
    // Task<AuthResponse> RefreshTokenAsync(string refreshToken);

    Task<AuthResponse>Register(User user);
    Task<AuthResponse?>Login(LoginRequest request);
    Task<AuthResponse?>RefreshToken(string refreshToken);
    Task<User?> GetUserByUsername(string username);
    Task<User?> GetUserById(int id);
    

} 