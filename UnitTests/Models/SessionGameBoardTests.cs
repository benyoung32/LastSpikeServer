using System;
using Xunit;
using GameplaySessionTracker.Models;

namespace GameplaySessionTracker.Tests.Models;

public class SessionGameBoardTests
{
    [Fact]
    public void SessionGameBoard_DefaultConstructor_InitializesProperties()
    {
        // Arrange & Act
        var sessionGameBoard = new GameBoard();

        // Assert
        Assert.Equal(Guid.Empty, sessionGameBoard.Id);
        Assert.Equal(Guid.Empty, sessionGameBoard.SessionId);
        Assert.Equal(string.Empty, sessionGameBoard.Data);
    }

    [Fact]
    public void SessionGameBoard_SetProperties_ReturnsCorrectValues()
    {
        // Arrange
        var sessionGameBoard = new GameBoard();
        var id = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var data = "Test Data";

        // Act
        sessionGameBoard.Id = id;
        sessionGameBoard.SessionId = sessionId;
        sessionGameBoard.Data = data;

        // Assert
        Assert.Equal(id, sessionGameBoard.Id);
        Assert.Equal(sessionId, sessionGameBoard.SessionId);
        Assert.Equal(data, sessionGameBoard.Data);
    }
}
