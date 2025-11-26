using GameplaySessionTracker.Controllers;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GameplaySessionTracker.Tests.Controllers;

public class MetricsControllerTests
{
    private readonly Mock<IMetricsService> _mockMetricsService;
    private readonly Mock<IPlayerService> _mockPlayerService;
    private readonly MetricsController _controller;

    public MetricsControllerTests()
    {
        _mockMetricsService = new Mock<IMetricsService>();
        _mockPlayerService = new Mock<IPlayerService>();
        _controller = new MetricsController(_mockMetricsService.Object, _mockPlayerService.Object);
    }

    [Fact]
    public void Get_ReturnsOkWithServiceMetrics()
    {
        // Arrange
        var serviceMetrics = new ServiceMetrics { Id = Guid.NewGuid() };
        _mockMetricsService.Setup(s => s.Get()).Returns(serviceMetrics);

        // Act
        var result = _controller.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(serviceMetrics, okResult.Value);
        _mockMetricsService.Verify(s => s.Get(), Times.Once);
    }

    [Fact]
    public void GetById_ExistingPlayer_ReturnsOkWithPlayerMetrics()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var player = new Player { Id = playerId, Name = "Test Player", Alias = "TP" };
        var playerMetrics = new PlayerMetrics { Id = playerId };

        _mockPlayerService.Setup(s => s.GetById(playerId)).Returns(player);
        _mockMetricsService.Setup(s => s.GetById(playerId)).Returns(playerMetrics);

        // Act
        var result = _controller.GetById(playerId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(playerMetrics, okResult.Value);
        _mockPlayerService.Verify(s => s.GetById(playerId), Times.Once);
        _mockMetricsService.Verify(s => s.GetById(playerId), Times.Once);
    }

    [Fact]
    public void GetById_NonExistentPlayer_ReturnsNotFound()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        _mockPlayerService.Setup(s => s.GetById(playerId)).Returns((Player?)null);

        // Act
        var result = _controller.GetById(playerId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        _mockPlayerService.Verify(s => s.GetById(playerId), Times.Once);
        _mockMetricsService.Verify(s => s.GetById(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public void Reset_ReturnsOkWithTrue()
    {
        // Arrange
        _mockMetricsService.Setup(s => s.Reset()).Returns(true);

        // Act
        var result = _controller.Reset();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.True((bool)okResult.Value!);
        _mockMetricsService.Verify(s => s.Reset(), Times.Once);
    }

    [Fact]
    public void ResetById_ExistingPlayer_ReturnsOkWithTrue()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var player = new Player { Id = playerId, Name = "Test Player", Alias = "TP" };

        _mockPlayerService.Setup(s => s.GetById(playerId)).Returns(player);
        _mockMetricsService.Setup(s => s.ResetById(playerId)).Returns(true);

        // Act
        var result = _controller.ResetById(playerId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.True((bool)okResult.Value!);
        _mockPlayerService.Verify(s => s.GetById(playerId), Times.Once);
        _mockMetricsService.Verify(s => s.ResetById(playerId), Times.Once);
    }

    [Fact]
    public void ResetById_NonExistentPlayer_ReturnsNotFound()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        _mockPlayerService.Setup(s => s.GetById(playerId)).Returns((Player?)null);

        // Act
        var result = _controller.ResetById(playerId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        _mockPlayerService.Verify(s => s.GetById(playerId), Times.Once);
        _mockMetricsService.Verify(s => s.ResetById(It.IsAny<Guid>()), Times.Never);
    }
}
