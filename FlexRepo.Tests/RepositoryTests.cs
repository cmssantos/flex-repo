using Xunit;
using FlexRepo.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FlexRepo.Tests;

public class RepositoryTests
{
    private readonly DbContextOptions<TestDbContext> _dbContextOptions;

    public RepositoryTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntity()
    {
        // Arrange
        using var context = new TestDbContext(_dbContextOptions);
        var repository = new Repository<TestUser, Guid, TestDbContext>(context);
        var entity = new TestUser { Id = Guid.NewGuid(), Name = "Test Entity" };

        // Act
        await repository.AddAsync(entity);
        await repository.SaveChangesAsync();

        // Assert
        var testUser = await context.TestUsers.FirstOrDefaultAsync();
        var countUsers = await context.TestUsers.CountAsync();

        Assert.Equal(1, countUsers);
        Assert.Equal("Test Entity", testUser?.Name);
    }

    [Fact]
    public async Task GetUserByEmailAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        using var context = new TestDbContext(_dbContextOptions);
        var repository = new Repository<TestUser, Guid, TestDbContext>(context);
        var testUser = new TestUser { Id = Guid.NewGuid(), Email = "test@example.com", Name = "Test User" };

        await repository.AddAsync(testUser, true);

        // Act
        var result = await repository.GetSingleOrDefaultAsync(u => u.Email == "test@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("Test User", result.Name);
    }

    [Fact]
    public async Task GetUserByEmailAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        using var context = new TestDbContext(_dbContextOptions);
        var repository = new Repository<TestUser, Guid, TestDbContext>(context);

        // Act
        var result = await repository.GetSingleOrDefaultAsync(u => u.Email == "nonexistent@example.com");

        // Assert
        Assert.Null(result);
    }
}