using GameplaySessionTracker.Models;
using GameplaySessionTracker.Repositories;
using GameplaySessionTracker.Services;
using Moq;
using Xunit;

namespace GameplaySessionTracker.Tests.Services;

public class SessionGameBoardServiceTests(
        GameBoardService service,
        Mock<IGameBoardRepository> mockRepository)
{
    [Fact]
    public void GetAll_ReturnsAllSessionGameBoards()
    {
        // Arrange
        var sessionGameBoards = new List<GameBoard>
        {
            new GameBoard { Id = Guid.NewGuid(), SessionId = Guid.NewGuid(), Data = "Data1" },
            new GameBoard { Id = Guid.NewGuid(), SessionId = Guid.NewGuid(), Data = "Data2" }
        };
        mockRepository.Setup(r => r.GetAll()).Returns(sessionGameBoards);

        // Act
        var result = service.GetAll().Result;

        // Assert
        Assert.Equal(2, result.Count());
        mockRepository.Verify(r => r.GetAll(), Times.Once);
    }

    [Fact]
    public void GetById_ExistingId_ReturnsSessionGameBoard()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sgb = new GameBoard { Id = id, SessionId = Guid.NewGuid(), Data = "Test" };
        mockRepository.Setup(r => r.GetById(id)).Returns(sgb);

        // Act
        var result = service.GetById(id).Result;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        mockRepository.Verify(r => r.GetById(id), Times.Once);
    }

    [Fact]
    public void GetById_NonExistentId_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        mockRepository.Setup(r => r.GetById(id)).Returns((GameBoard?)null);

        // Act
        var result = service.GetById(id);

        // Assert
        Assert.Null(result);
        mockRepository.Verify(r => r.GetById(id), Times.Once);
    }

    [Fact]
    public void Create_ValidSessionGameBoard_CallsRepositoryAdd()
    {
        // Arrange
        var sgb = new GameBoard { Id = Guid.NewGuid(), SessionId = Guid.NewGuid(), Data = "New" };

        // Act
        var result = service.Create(sgb).Result;

        // Assert
        Assert.Equal(sgb, result);
        mockRepository.Verify(r => r.Add(sgb), Times.Once);
    }

    [Fact]
    public void Update_ValidSessionGameBoard_CallsRepositoryUpdate()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sgb = new GameBoard { Id = id, SessionId = Guid.NewGuid(), Data = "Updated" };
        mockRepository.Setup(r => r.GetById(id)).Returns(sgb);

        // Act
        service.Update(id, sgb);

        // Assert
        mockRepository.Verify(r => r.Update(sgb), Times.Once);
    }

    [Fact]
    public void Delete_ValidId_CallsRepositoryDelete()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        service.Delete(id);

        // Assert
        mockRepository.Verify(r => r.Delete(id), Times.Once);
    }
}
