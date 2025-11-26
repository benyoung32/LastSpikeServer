using System.Threading.Tasks;
using GameplaySessionTracker.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace GameplaySessionTracker.Tests.Hubs;

public class GameHubTests
{
    private readonly Mock<HubCallerContext> _mockContext;
    private readonly Mock<IGroupManager> _mockGroups;
    private readonly GameHub _hub;

    public GameHubTests()
    {
        _mockContext = new Mock<HubCallerContext>();
        _mockGroups = new Mock<IGroupManager>();

        _hub = new GameHub
        {
            Context = _mockContext.Object,
            Groups = _mockGroups.Object
        };

        _mockContext.Setup(c => c.ConnectionId).Returns("test-connection-id");
    }

    [Fact]
    public async Task OnConnectedAsync_CompletesSuccessfully()
    {
        // Act
        await _hub.OnConnectedAsync();

        // Assert
        // Verify that the method completes without throwing exceptions
        Assert.True(true);
    }

    [Fact]
    public async Task OnDisconnectedAsync_WithoutException_CompletesSuccessfully()
    {
        // Act
        await _hub.OnDisconnectedAsync(null);

        // Assert
        // Verify that the method completes without throwing exceptions
        Assert.True(true);
    }

    [Fact]
    public async Task OnDisconnectedAsync_WithException_CompletesSuccessfully()
    {
        // Arrange
        var exception = new Exception("Test exception");

        // Act
        await _hub.OnDisconnectedAsync(exception);

        // Assert
        // Verify that the method completes without throwing exceptions
        Assert.True(true);
    }

    [Fact]
    public async Task JoinSession_AddsToGroup()
    {
        // Arrange
        var sessionId = "test-session-123";
        var expectedGroupName = $"session_{sessionId}";

        _mockGroups.Setup(g => g.AddToGroupAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.JoinSession(sessionId);

        // Assert
        _mockGroups.Verify(g => g.AddToGroupAsync(
            "test-connection-id",
            expectedGroupName,
            default), Times.Once);
    }

    [Fact]
    public async Task LeaveSession_RemovesFromGroup()
    {
        // Arrange
        var sessionId = "test-session-123";
        var expectedGroupName = $"session_{sessionId}";

        _mockGroups.Setup(g => g.RemoveFromGroupAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.LeaveSession(sessionId);

        // Assert
        _mockGroups.Verify(g => g.RemoveFromGroupAsync(
            "test-connection-id",
            expectedGroupName,
            default), Times.Once);
    }
}
