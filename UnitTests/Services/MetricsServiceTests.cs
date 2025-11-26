using GameplaySessionTracker.Models;
using GameplaySessionTracker.Services;
using Xunit;

namespace GameplaySessionTracker.Tests.Services;

public class MetricsServiceTests
{
    private readonly MetricsService _service;

    public MetricsServiceTests()
    {
        _service = new MetricsService();
    }

    [Fact]
    public void Get_ReturnsServiceMetrics()
    {
        // Act
        var result = _service.Get();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ServiceMetrics>(result);
    }

    [Fact]
    public void GetById_WithValidId_ReturnsPlayerMetrics()
    {
        // Arrange
        var playerId = Guid.NewGuid();

        // Act
        var result = _service.GetById(playerId);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<PlayerMetrics>(result);
    }

    [Fact]
    public void Reset_ReturnsTrue()
    {
        // Act
        var result = _service.Reset();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ResetById_WithValidId_ReturnsTrue()
    {
        // Arrange
        var playerId = Guid.NewGuid();

        // Act
        var result = _service.ResetById(playerId);

        // Assert
        Assert.True(result);
    }
}
