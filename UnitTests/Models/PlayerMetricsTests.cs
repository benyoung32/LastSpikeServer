using GameplaySessionTracker.Models;
using Xunit;

namespace GameplaySessionTracker.Tests.Models;

public class PlayerMetricsTests
{
    [Fact]
    public void Id_CanBeSet_AndGet()
    {
        // Arrange
        var playerMetrics = new PlayerMetrics();
        var expectedId = Guid.NewGuid();

        // Act
        playerMetrics.Id = expectedId;

        // Assert
        Assert.Equal(expectedId, playerMetrics.Id);
    }

    [Fact]
    public void Constructor_InitializesWithDefaultId()
    {
        // Act
        var playerMetrics = new PlayerMetrics();

        // Assert
        Assert.Equal(Guid.Empty, playerMetrics.Id);
    }

    [Fact]
    public void Id_DefaultValue_IsEmpty()
    {
        // Arrange & Act
        var playerMetrics = new PlayerMetrics();

        // Assert
        Assert.Equal(Guid.Empty, playerMetrics.Id);
    }
}
