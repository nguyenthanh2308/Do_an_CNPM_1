using Moq;
using AutoMapper;
using FluentAssertions;
using HotelManagement.API.Models.DTOs.Auth;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Repositories.Interfaces;
using HotelManagement.API.Services.Implementations;
using HotelManagement.API.Exceptions;
using Microsoft.Extensions.Configuration;

namespace HotelManagement.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUoW;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<IUserRepository> _mockUsers;
    private readonly Mock<IRefreshTokenRepository> _mockTokens;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _mockUoW    = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockConfig = new Mock<IConfiguration>();

        _mockUsers  = new Mock<IUserRepository>();
        _mockTokens = new Mock<IRefreshTokenRepository>();

        _mockUoW.Setup(u => u.Users).Returns(_mockUsers.Object);
        _mockUoW.Setup(u => u.RefreshTokens).Returns(_mockTokens.Object);
        _mockUoW.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Provide valid JWT settings
        _mockConfig.Setup(c => c["Jwt:Secret"]).Returns("SuperSecretKeyForTestingThatIsAtLeast32Chars!");
        _mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("HotelManagementAPI");
        _mockConfig.Setup(c => c["Jwt:Audience"]).Returns("HotelManagementClient");
        _mockConfig.Setup(c => c["Jwt:ExpirationHours"]).Returns("24");

        _sut = new AuthService(_mockUoW.Object, _mockMapper.Object, _mockConfig.Object);
    }

    // ─── LoginAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_EmptyUsername_ThrowsValidationException()
    {
        var dto = new LoginRequestDto { Username = "", Password = "Password123!" };

        Func<Task> act = () => _sut.LoginAsync(dto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*required*");
    }

    [Fact]
    public async Task LoginAsync_EmptyPassword_ThrowsValidationException()
    {
        var dto = new LoginRequestDto { Username = "admin", Password = "" };

        Func<Task> act = () => _sut.LoginAsync(dto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsUnauthorizedException()
    {
        _mockUsers.Setup(r => r.GetUserByUsernameAsync("ghost"))
                  .ReturnsAsync((User?)null);

        var dto = new LoginRequestDto { Username = "ghost", Password = "Password123!" };

        Func<Task> act = () => _sut.LoginAsync(dto);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*Invalid username or password*");
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedException()
    {
        var user = new User
        {
            Id           = 1,
            Username     = "manager",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword!"),
            Role         = "Manager",
            Email        = "m@hotel.com",
            CreatedAt    = DateTime.UtcNow
        };
        _mockUsers.Setup(r => r.GetUserByUsernameAsync("manager")).ReturnsAsync(user);

        var dto = new LoginRequestDto { Username = "manager", Password = "WrongPassword!" };

        Func<Task> act = () => _sut.LoginAsync(dto);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*Invalid username or password*");
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsLoginResponse()
    {
        const string rawPassword  = "Password123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(rawPassword);
        var user = new User
        {
            Id           = 1,
            Username     = "admin",
            PasswordHash = hashedPassword,
            Role         = "Admin",
            Email        = "admin@hotel.com",
            CreatedAt    = DateTime.UtcNow
        };

        _mockUsers.Setup(r => r.GetUserByUsernameAsync("admin")).ReturnsAsync(user);
        _mockTokens.Setup(r => r.AddAsync(It.IsAny<RefreshToken>())).ReturnsAsync(new RefreshToken());

        var userDto = new UserDto { Id = 1, Username = "admin", Role = "Admin" };
        _mockMapper.Setup(m => m.Map<UserDto>(user)).Returns(userDto);

        var dto = new LoginRequestDto { Username = "admin", Password = rawPassword };

        var result = await _sut.LoginAsync(dto);

        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Should().BeEquivalentTo(userDto);
    }

    // ─── RegisterAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_EmptyUsername_ThrowsValidationException()
    {
        var dto = new RegisterRequestDto { Username = "", Email = "a@b.com", Password = "Pass123!" };

        Func<Task> act = () => _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task RegisterAsync_ShortPassword_ThrowsValidationException()
    {
        var dto = new RegisterRequestDto { Username = "user1", Email = "a@b.com", Password = "abc" };

        Func<Task> act = () => _sut.RegisterAsync(dto);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.And.Errors.Should().Contain(e => e.Contains("6 characters"));
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ThrowsValidationException()
    {
        var existingUser = new User { Username = "existinguser", Email = "e@b.com" };
        _mockUsers.Setup(r => r.GetUserByUsernameAsync("existinguser")).ReturnsAsync(existingUser);

        var dto = new RegisterRequestDto
        {
            Username = "existinguser",
            Email    = "new@b.com",
            Password = "Password123!"
        };

        Func<Task> act = () => _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Username already exists*");
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsValidationException()
    {
        _mockUsers.Setup(r => r.GetUserByUsernameAsync("newuser")).ReturnsAsync((User?)null);
        var existingEmail = new User { Username = "other", Email = "used@b.com" };
        _mockUsers.Setup(r => r.GetUserByEmailAsync("used@b.com")).ReturnsAsync(existingEmail);

        var dto = new RegisterRequestDto
        {
            Username = "newuser",
            Email    = "used@b.com",
            Password = "Password123!"
        };

        Func<Task> act = () => _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Email already exists*");
    }

    // ─── GetCurrentUserAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetCurrentUserAsync_UserNotFound_ThrowsNotFoundException()
    {
        _mockUsers.Setup(r => r.GetByIdAsync<long>(99)).ReturnsAsync((User?)null);

        Func<Task> act = () => _sut.GetCurrentUserAsync(99);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*User*99*");
    }

    [Fact]
    public async Task GetCurrentUserAsync_ValidId_ReturnsUserDto()
    {
        var user = new User
        {
            Id = 5, Username = "guest", Email = "g@hotel.com", Role = "Customer", CreatedAt = DateTime.UtcNow
        };
        var dto = new UserDto { Id = 5, Username = "guest", Role = "Customer" };

        _mockUsers.Setup(r => r.GetByIdAsync<long>(5)).ReturnsAsync(user);
        _mockMapper.Setup(m => m.Map<UserDto>(user)).Returns(dto);

        var result = await _sut.GetCurrentUserAsync(5);

        result.Should().BeEquivalentTo(dto);
    }

    // ─── ValidateTokenAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task ValidateTokenAsync_InvalidToken_ReturnsFalse()
    {
        var result = await _sut.ValidateTokenAsync("not-a-real-jwt");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateTokenAsync_EmptyToken_ReturnsFalse()
    {
        var result = await _sut.ValidateTokenAsync("");

        result.Should().BeFalse();
    }

    // ─── LogoutAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task LogoutAsync_RemovesAllRefreshTokensForUser()
    {
        var tokens = new List<RefreshToken>
        {
            new() { TokenId = 1, UserId = 3, Token = "tok1", ExpiresAt = DateTime.Now.AddDays(7), CreatedAt = DateTime.Now },
            new() { TokenId = 2, UserId = 3, Token = "tok2", ExpiresAt = DateTime.Now.AddDays(7), CreatedAt = DateTime.Now }
        };

        _mockTokens
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
            .ReturnsAsync(tokens);

        await _sut.LogoutAsync(3);

        _mockTokens.Verify(r => r.DeleteAsync<int>(It.IsAny<int>()), Times.Exactly(2));
        _mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
