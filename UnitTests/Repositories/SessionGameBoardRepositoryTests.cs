using GameplaySessionTracker.Models;
using GameplaySessionTracker.Repositories;
using Xunit;

namespace GameplaySessionTracker.Tests.Repositories;

public class SessionGameBoardRepositoryTests
{
    [Fact]
    public void GetAll_InitiallyEmpty_ReturnsEmptyCollection()
    {
        // Arrange
        var repository = new GameBoardRepository();

        // Act
        var result = repository.GetAll();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Add_AddsSessionGameBoard_CanBeRetrieved()
    {
        // Arrange
        var repository = new GameBoardRepository();
        var sessionGameBoard = new GameBoard
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            Data = "Test Data"
        };

        // Act
        repository.Add(sessionGameBoard);
        var result = repository.GetById(sessionGameBoard.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(sessionGameBoard.Id, result.Id);
        Assert.Equal(sessionGameBoard.SessionId, result.SessionId);
        Assert.Equal(sessionGameBoard.Data, result.Data);
    }

    [Fact]
    public void GetById_NonExistent_ReturnsNull()
    {
        // Arrange
        var repository = new GameBoardRepository();

        // Act
        var result = repository.GetById(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Update_ExistingSessionGameBoard_UpdatesData()
    {
        // Arrange
        var repository = new GameBoardRepository();
        var sessionGameBoard = new GameBoard
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            Data = "Original Data"
        };
        repository.Add(sessionGameBoard);

        // Act
        sessionGameBoard.Data = "Updated Data";
        repository.Update(sessionGameBoard);
        var result = repository.GetById(sessionGameBoard.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Data", result.Data);
    }

    [Fact]
    public void Update_NonExistent_DoesNothing()
    {
        // Arrange
        var repository = new GameBoardRepository();
        var sessionGameBoard = new GameBoard
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            Data = "Test Data"
        };

        // Act
        repository.Update(sessionGameBoard);
        var result = repository.GetById(sessionGameBoard.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Delete_ExistingSessionGameBoard_RemovesFromCollection()
    {
        // Arrange
        var repository = new GameBoardRepository();
        var sessionGameBoard = new GameBoard
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            Data = "Test Data"
        };
        repository.Add(sessionGameBoard);

        // Act
        repository.Delete(sessionGameBoard.Id);
        var result = repository.GetById(sessionGameBoard.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Delete_NonExistent_DoesNothing()
    {
        // Arrange
        var repository = new GameBoardRepository();

        // Act & Assert (no exception should be thrown)
        repository.Delete(Guid.NewGuid());
    }

    [Fact]
    public void GetAll_MultipleItems_ReturnsAll()
    {
        // Arrange
        var repository = new GameBoardRepository();
        var sgb1 = new GameBoard { Id = Guid.NewGuid(), SessionId = Guid.NewGuid(), Data = "Data1" };
        var sgb2 = new GameBoard { Id = Guid.NewGuid(), SessionId = Guid.NewGuid(), Data = "Data2" };

        // Act
        repository.Add(sgb1);
        repository.Add(sgb2);
        var result = repository.GetAll().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, s => s.Id == sgb1.Id);
        Assert.Contains(result, s => s.Id == sgb2.Id);
    }
}
