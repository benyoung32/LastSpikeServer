using GameplaySessionTracker.Models;
using Xunit;

namespace GameplaySessionTracker.Tests.Models;

public class ServiceMetricsTests
{
    [Fact]
    public void Id_CanBeSet_AndGet()
    {
        // Arrange
        var serviceMetrics = new ServiceMetrics();
        var expectedId = Guid.NewGuid();

        // Act
        serviceMetrics.Id = expectedId;

        // Assert
        Assert.Equal(expectedId, serviceMetrics.Id);
    }

    [Fact]
    public void Constructor_InitializesWithDefaultId()
    {
        // Act
        var serviceMetrics = new ServiceMetrics();

        // Assert
        Assert.Equal(Guid.Empty, serviceMetrics.Id);
    }

    [Fact]
    public void Id_DefaultValue_IsEmpty()
    {
        // Arrange & Act
        var serviceMetrics = new ServiceMetrics();

        // Assert
        Assert.Equal(Guid.Empty, serviceMetrics.Id);
    }
}
