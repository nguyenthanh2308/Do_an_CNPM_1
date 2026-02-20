using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using HotelManagement.API.Data;
using HotelManagement.API.Models.Entities;
using HotelManagement.API.Repositories.Implementations;

namespace HotelManagement.Tests.Repositories;

public class RepositoryTests
{
    private HotelDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new HotelDbContext(options);
    }

    [Fact]
    public async Task GenericRepository_AddAsync_AddsEntity()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new GenericRepository<Guest>(context);
        var guest = new Guest
        {
            FullName = "John Doe",
            Email = "john@example.com",
            Phone = "1234567890",
            IdNumber = "ID123456",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await repository.AddAsync(guest);
        await context.SaveChangesAsync();

        // Assert
        var savedGuest = await context.Guests.FirstOrDefaultAsync();
        savedGuest.Should().NotBeNull();
        savedGuest!.FullName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GenericRepository_GetByIdAsync_ReturnsEntity()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var guest = new Guest
        {
            Id = 1,
            FullName = "Jane Doe",
            Email = "jane@example.com",
            Phone = "0987654321",
            IdNumber = "ID654321",
            CreatedAt = DateTime.UtcNow
        };
        context.Guests.Add(guest);
        await context.SaveChangesAsync();

        var repository = new GenericRepository<Guest>(context);

        // Act
        var result = await repository.GetByIdAsync(1L);

        // Assert
        result.Should().NotBeNull();
        result!.FullName.Should().Be("Jane Doe");
    }

    [Fact]
    public async Task GenericRepository_GetAllAsync_ReturnsAllEntities()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        context.Guests.AddRange(
            new Guest { FullName = "Guest 1", Email = "g1@test.com", Phone = "111", IdNumber = "ID1", CreatedAt = DateTime.UtcNow },
            new Guest { FullName = "Guest 2", Email = "g2@test.com", Phone = "222", IdNumber = "ID2", CreatedAt = DateTime.UtcNow },
            new Guest { FullName = "Guest 3", Email = "g3@test.com", Phone = "333", IdNumber = "ID3", CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var repository = new GenericRepository<Guest>(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GenericRepository_UpdateAsync_UpdatesEntity()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var guest = new Guest
        {
            Id = 1,
            FullName = "Original Name",
            Email = "original@example.com",
            Phone = "1111111111",
            IdNumber = "ID111",
            CreatedAt = DateTime.UtcNow
        };
        context.Guests.Add(guest);
        await context.SaveChangesAsync();

        var repository = new GenericRepository<Guest>(context);

        // Act
        guest.FullName = "Updated Name";
        repository.Update(guest);
        await context.SaveChangesAsync();

        // Assert
        var updated = await context.Guests.FindAsync(1L);
        updated!.FullName.Should().Be("Updated Name");
    }

    [Fact]
    public async Task GenericRepository_DeleteAsync_RemovesEntity()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var guest = new Guest
        {
            Id = 1,
            FullName = "To Delete",
            Email = "delete@example.com",
            Phone = "9999999999",
            IdNumber = "ID999",
            CreatedAt = DateTime.UtcNow
        };
        context.Guests.Add(guest);
        await context.SaveChangesAsync();

        var repository = new GenericRepository<Guest>(context);

        // Act
        await repository.DeleteAsync(guest.Id);
        await context.SaveChangesAsync();

        // Assert
        var deleted = await context.Guests.FindAsync(1L);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task UserRepository_GetByUsernameAsync_ReturnsUser()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            PasswordHash = "hashedpassword",
            Role = "Customer",
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repository = new UserRepository(context);

        // Act
        var result = await repository.GetUserByUsernameAsync("testuser");

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task UserRepository_GetByEmailAsync_ReturnsUser()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var user = new User
        {
            Id = 1,
            Username = "emailuser",
            PasswordHash = "hashedpassword",
            Role = "Customer",
            Email = "unique@example.com",
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repository = new UserRepository(context);

        // Act
        var result = await repository.GetUserByEmailAsync("unique@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("unique@example.com");
    }
}
