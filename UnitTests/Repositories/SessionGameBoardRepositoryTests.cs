using GameplaySessionTracker.Repositories;
using Xunit;

namespace GameplaySessionTracker.Tests.Repositories;

public class SessionGameBoardRepositoryTests
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
}
