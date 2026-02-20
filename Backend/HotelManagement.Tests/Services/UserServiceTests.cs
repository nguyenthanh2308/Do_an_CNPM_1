using Moq;
using FluentAssertions;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Repositories.Interfaces;
using HotelManagement.API.Services.Implementations;
using HotelManagement.API.Exceptions;

namespace HotelManagement.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUnitOfWork>    _mockUoW;
    private readonly Mock<IUserRepository> _mockUsers;
    private readonly UserService          _sut;

    public UserServiceTests()
    {
        _mockUoW   = new Mock<IUnitOfWork>();
        _mockUsers = new Mock<IUserRepository>();

        _mockUoW.Setup(u => u.Users).Returns(_mockUsers.Object);
        _mockUoW.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        _sut = new UserService(_mockUoW.Object);
    }

    // ─── GetAllUsersAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllUsersAsync_ReturnsAllUsers()
    {
        var users = new List<User>
        {
            new() { Id = 1, Username = "admin",   Role = "Admin",   Email = "a@h.com", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Username = "manager", Role = "Manager", Email = "m@h.com", CreatedAt = DateTime.UtcNow }
        };
        _mockUsers.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        var result = await _sut.GetAllUsersAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllUsersAsync_EmptyDB_ReturnsEmptyList()
    {
        _mockUsers.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User>());

        var result = await _sut.GetAllUsersAsync();

        result.Should().BeEmpty();
    }

    // ─── GetUserByIdAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserByIdAsync_UserExists_ReturnsUser()
    {
        var user = new User { Id = 5, Username = "receptionist", Role = "Receptionist", Email = "r@h.com", CreatedAt = DateTime.UtcNow };
        _mockUsers.Setup(r => r.GetByIdAsync<long>(5)).ReturnsAsync(user);

        var result = await _sut.GetUserByIdAsync(5);

        result.Should().NotBeNull();
        result!.Username.Should().Be("receptionist");
    }

    [Fact]
    public async Task GetUserByIdAsync_UserNotFound_ReturnsNull()
    {
        _mockUsers.Setup(r => r.GetByIdAsync<long>(99)).ReturnsAsync((User?)null);

        var result = await _sut.GetUserByIdAsync(99);

        result.Should().BeNull();
    }

    // ─── GetUserByUsernameAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetUserByUsernameAsync_Exists_ReturnsUser()
    {
        var user = new User { Id = 10, Username = "housekeeping1", Role = "Housekeeping", Email = "h@h.com", CreatedAt = DateTime.UtcNow };
        _mockUsers.Setup(r => r.GetUserByUsernameAsync("housekeeping1")).ReturnsAsync(user);

        var result = await _sut.GetUserByUsernameAsync("housekeeping1");

        result.Should().NotBeNull();
        result!.Role.Should().Be("Housekeeping");
    }

    [Fact]
    public async Task GetUserByUsernameAsync_NotFound_ReturnsNull()
    {
        _mockUsers.Setup(r => r.GetUserByUsernameAsync("ghost")).ReturnsAsync((User?)null);

        var result = await _sut.GetUserByUsernameAsync("ghost");

        result.Should().BeNull();
    }

    // ─── CreateUserAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateUserAsync_DuplicateUsername_ThrowsValidationException()
    {
        var existing = new User { Id = 1, Username = "admin", Email = "a@h.com", CreatedAt = DateTime.UtcNow };
        _mockUsers.Setup(r => r.GetUserByUsernameAsync("admin")).ReturnsAsync(existing);

        var newUser = new User { Username = "admin", Email = "new@h.com" };

        Func<Task> act = () => _sut.CreateUserAsync(newUser, "Password123!");

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Username already exists*");
    }

    [Fact]
    public async Task CreateUserAsync_DuplicateEmail_ThrowsValidationException()
    {
        _mockUsers.Setup(r => r.GetUserByUsernameAsync("newuser")).ReturnsAsync((User?)null);
        var existing = new User { Id = 2, Username = "other", Email = "used@h.com", CreatedAt = DateTime.UtcNow };
        _mockUsers.Setup(r => r.GetUserByEmailAsync("used@h.com")).ReturnsAsync(existing);

        var newUser = new User { Username = "newuser", Email = "used@h.com" };

        Func<Task> act = () => _sut.CreateUserAsync(newUser, "Password123!");

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Email already exists*");
    }

    [Fact]
    public async Task CreateUserAsync_ValidInput_HashesPasswordAndSaves()
    {
        _mockUsers.Setup(r => r.GetUserByUsernameAsync("newstaff")).ReturnsAsync((User?)null);
        _mockUsers.Setup(r => r.GetUserByEmailAsync("s@h.com")).ReturnsAsync((User?)null);
        _mockUsers.Setup(r => r.AddAsync(It.IsAny<User>())).Returns<User>(u => Task.FromResult(u));

        var newUser = new User { Username = "newstaff", Email = "s@h.com", Role = "Receptionist" };

        var result = await _sut.CreateUserAsync(newUser, "SecurePassword123!");

        result.Should().NotBeNull();
        result.PasswordHash.Should().NotBe("SecurePassword123!"); // Must be hashed
        result.PasswordHash.Should().NotBeNullOrEmpty();
        _mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_SetsCreatedAt()
    {
        _mockUsers.Setup(r => r.GetUserByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _mockUsers.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _mockUsers.Setup(r => r.AddAsync(It.IsAny<User>())).Returns<User>(u => Task.FromResult(u));

        var newUser = new User { Username = "staff2", Email = "s2@h.com", Role = "Receptionist" };
        var before  = DateTime.Now;

        var result = await _sut.CreateUserAsync(newUser, "Password!");

        result.CreatedAt.Should().BeOnOrAfter(before);
    }

    // ─── UpdateUserAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateUserAsync_UserNotFound_ThrowsNotFoundException()
    {
        _mockUsers.Setup(r => r.GetByIdAsync<long>(77)).ReturnsAsync((User?)null);

        var updateData = new User { Email = "new@h.com", Role = "Manager" };

        Func<Task> act = () => _sut.UpdateUserAsync(77, updateData);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*User*77*");
    }

    [Fact]
    public async Task UpdateUserAsync_UserExists_UpdatesEmailAndRole()
    {
        var existing = new User { Id = 3, Username = "staff", Email = "old@h.com", Role = "Receptionist", CreatedAt = DateTime.UtcNow };
        _mockUsers.Setup(r => r.GetByIdAsync<long>(3)).ReturnsAsync(existing);

        var updateData = new User { Email = "new@h.com", Role = "Manager" };

        var result = await _sut.UpdateUserAsync(3, updateData);

        result.Email.Should().Be("new@h.com");
        result.Role.Should().Be("Manager");
        _mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ─── DeleteUserAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteUserAsync_UserNotFound_ThrowsNotFoundException()
    {
        _mockUsers.Setup(r => r.GetByIdAsync<long>(88)).ReturnsAsync((User?)null);

        Func<Task> act = () => _sut.DeleteUserAsync(88);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*User*88*");
    }

    [Fact]
    public async Task DeleteUserAsync_UserExists_DeletesAndReturnsTrue()
    {
        var user = new User { Id = 4, Username = "todelete", Email = "x@h.com", Role = "Customer", CreatedAt = DateTime.UtcNow };
        _mockUsers.Setup(r => r.GetByIdAsync<long>(4)).ReturnsAsync(user);

        var result = await _sut.DeleteUserAsync(4);

        result.Should().BeTrue();
        _mockUsers.Verify(r => r.DeleteAsync<long>(4), Times.Once);
        _mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ─── DeactivateUserAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task DeactivateUserAsync_UserNotFound_ThrowsNotFoundException()
    {
        _mockUsers.Setup(r => r.GetByIdAsync<long>(55)).ReturnsAsync((User?)null);

        Func<Task> act = () => _sut.DeactivateUserAsync(55);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeactivateUserAsync_UserExists_ReturnsTrue()
    {
        var user = new User { Id = 6, Username = "deactivate", Email = "d@h.com", Role = "Customer", CreatedAt = DateTime.UtcNow };
        _mockUsers.Setup(r => r.GetByIdAsync<long>(6)).ReturnsAsync(user);

        var result = await _sut.DeactivateUserAsync(6);

        result.Should().BeTrue();
    }

    // ─── GetUsersByRoleAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetUsersByRoleAsync_ReturnsOnlyMatchingRole()
    {
        var managers = new List<User>
        {
            new() { Id = 10, Username = "mgr1", Role = "Manager", Email = "mgr1@h.com", CreatedAt = DateTime.UtcNow },
            new() { Id = 11, Username = "mgr2", Role = "Manager", Email = "mgr2@h.com", CreatedAt = DateTime.UtcNow }
        };
        _mockUsers.Setup(r => r.GetUsersByRoleAsync("Manager")).ReturnsAsync(managers);

        var result = await _sut.GetUsersByRoleAsync("Manager");

        result.Should().HaveCount(2);
        result.All(u => u.Role == "Manager").Should().BeTrue();
    }
}
