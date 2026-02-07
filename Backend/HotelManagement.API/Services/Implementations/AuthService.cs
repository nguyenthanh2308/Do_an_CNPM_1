using AutoMapper;
using HotelManagement.API.Models.DTOs.Auth;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Repositories.Interfaces;
using HotelManagement.API.Services.Interfaces;
using HotelManagement.API.Exceptions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HotelManagement.API.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public AuthService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("Username and password are required.");
        }

        // Find user by username
        var user = await _unitOfWork.Users.GetUserByUsernameAsync(request.Username);
        if (user == null)
        {
            throw new UnauthorizedException("Invalid username or password.");
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid username or password.");
        }

        // Generate tokens
        var accessToken = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        // Save refresh token to database
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.Now.AddDays(7),
            CreatedAt = DateTime.Now
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();

        // Prepare response
        var userDto = _mapper.Map<UserDto>(user);
        var expiresAt = DateTime.Now.AddHours(GetJwtExpirationHours());

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = userDto
        };
    }

    public async Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // Validate input
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(request.Username))
            errors.Add("Username is required.");
        if (string.IsNullOrWhiteSpace(request.Email))
            errors.Add("Email is required.");
        if (string.IsNullOrWhiteSpace(request.Password))
            errors.Add("Password is required.");
        if (request.Password != null && request.Password.Length < 6)
            errors.Add("Password must be at least 6 characters long.");

        if (errors.Any())
            throw new ValidationException(errors);

        // Check if username already exists
        var existingUser = await _unitOfWork.Users.GetUserByUsernameAsync(request.Username);
        if (existingUser != null)
        {
            throw new ValidationException("Username already exists.");
        }

        // Check if email already exists
        var existingEmail = await _unitOfWork.Users.GetUserByEmailAsync(request.Email);
        if (existingEmail != null)
        {
            throw new ValidationException("Email already exists.");
        }

        // Create new user
        var user = _mapper.Map<User>(request);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.Role = "Customer"; // Default role for registration
        user.CreatedAt = DateTime.Now;

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Auto-login after registration
        var loginRequest = new LoginRequestDto
        {
            Username = request.Username ?? string.Empty,
            Password = request.Password ?? string.Empty
        };

        return await LoginAsync(loginRequest);
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new ValidationException("Refresh token is required.");
        }

        // Find refresh token in database
        var tokenEntity = (await _unitOfWork.RefreshTokens.FindAsync(t => t.Token == refreshToken))
            .FirstOrDefault();

        if (tokenEntity == null)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        // Check if token is expired
        if (tokenEntity.ExpiresAt < DateTime.Now)
        {
            throw new UnauthorizedException("Refresh token has expired.");
        }

        // Get user
        var user = await _unitOfWork.Users.GetByIdAsync(tokenEntity.UserId);
        if (user == null)
        {
            throw new NotFoundException("User", tokenEntity.UserId);
        }

        // Generate new access token
        var accessToken = GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();

        // Update refresh token
        tokenEntity.Token = newRefreshToken;
        tokenEntity.ExpiresAt = DateTime.Now.AddDays(7);
        await _unitOfWork.RefreshTokens.UpdateAsync(tokenEntity);
        await _unitOfWork.SaveChangesAsync();

        // Prepare response
        var userDto = _mapper.Map<UserDto>(user);
        var expiresAt = DateTime.Now.AddHours(GetJwtExpirationHours());

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = expiresAt,
            User = userDto
        };
    }

    public async Task<UserDto> GetCurrentUserAsync(long userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        return _mapper.Map<UserDto>(user);
    }

    public async Task LogoutAsync(long userId)
    {
        // Remove all refresh tokens for this user
        var tokens = await _unitOfWork.RefreshTokens.FindAsync(t => t.UserId == userId);
        foreach (var token in tokens)
        {
            await _unitOfWork.RefreshTokens.DeleteAsync(token.TokenId);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(GetJwtSecret());

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = GetJwtIssuer(),
                ValidateAudience = true,
                ValidAudience = GetJwtAudience(),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    // ========== Private Helper Methods ==========

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(GetJwtSecret());

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(GetJwtExpirationHours()),
            Issuer = GetJwtIssuer(),
            Audience = GetJwtAudience(),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private string GetJwtSecret()
    {
        return _configuration["Jwt:Secret"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
    }

    private string GetJwtIssuer()
    {
        return _configuration["Jwt:Issuer"] ?? "HotelManagementAPI";
    }

    private string GetJwtAudience()
    {
        return _configuration["Jwt:Audience"] ?? "HotelManagementClient";
    }

    private int GetJwtExpirationHours()
    {
        return int.TryParse(_configuration["Jwt:ExpirationHours"], out var hours) ? hours : 24;
    }
}
