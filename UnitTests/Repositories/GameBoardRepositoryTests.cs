using GameplaySessionTracker.Repositories;
using Xunit;

namespace GameplaySessionTracker.Tests.Repositories;

public class GameBoardRepositoryTests
{
    private const string TestConnectionString = "Server=test;Database=test;";

    [Fact]
    public void Constructor_SetsConnectionString()
    {
        // Arrange & Act
        var repository = new GameBoardRepository(TestConnectionString);

        // Assert
        Assert.NotNull(repository);
    }

    [Fact]
    public void Constructor_WithNullConnectionString_CreatesRepository()
    {
        // Arrange
        string? connectionString = null;

        // Act
        var repository = new GameBoardRepository(connectionString!);

        // Assert
        Assert.NotNull(repository);
    }

    [Fact]
    public void Constructor_WithEmptyConnectionString_CreatesRepository()
    {
        // Arrange
        var connectionString = string.Empty;

        // Act
        var repository = new GameBoardRepository(connectionString);

        // Assert
        Assert.NotNull(repository);
    }
}
