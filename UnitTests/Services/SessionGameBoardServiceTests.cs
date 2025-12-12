using GameplaySessionTracker.Models;
using GameplaySessionTracker.Repositories;
using GameplaySessionTracker.Services;
using GameplaySessionTracker.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace GameplaySessionTracker.Tests.Services;

public class SessionGameBoardServiceTests
{
    private readonly Mock<IGameBoardRepository> _mockRepository;
    private readonly Mock<IHubContext<GameHub>> _mockHubContext;
    private readonly Mock<IHubClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly GameBoardService _service;

    public SessionGameBoardServiceTests()
    {
        _mockRepository = new Mock<IGameBoardRepository>();
        _mockHubContext = new Mock<IHubContext<GameHub>>();
        _mockClients = new Mock<IHubClients>();
        _mockClientProxy = new Mock<IClientProxy>();

        _mockHubContext.Setup(h => h.Clients).Returns(_mockClients.Object);
        _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);

        _service = new GameBoardService(_mockRepository.Object, _mockHubContext.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsAllSessionGameBoards()
    {
        // Arrange
        var sessionGameBoards = new List<GameBoard>
        {
            new GameBoard { Id = Guid.NewGuid(), SessionId = Guid.NewGuid(), Data = "Data1" },
            new GameBoard { Id = Guid.NewGuid(), SessionId = Guid.NewGuid(), Data = "Data2" }
        };
        _mockRepository.Setup(r => r.GetAll()).ReturnsAsync(sessionGameBoards);

        // Act
        var result = await _service.GetAll();

        // Assert
        Assert.Equal(2, result.Count());
        _mockRepository.Verify(r => r.GetAll(), Times.Once);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsSessionGameBoard()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sgb = new GameBoard { Id = id, SessionId = Guid.NewGuid(), Data = "Test" };
        _mockRepository.Setup(r => r.GetById(id)).ReturnsAsync(sgb);

        // Act
        var result = await _service.GetById(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        _mockRepository.Verify(r => r.GetById(id), Times.Once);
    }

    [Fact]
    public async Task GetById_NonExistentId_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetById(id)).ReturnsAsync((GameBoard?)null);

        // Act
        var result = await _service.GetById(id);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetById(id), Times.Once);
    }

    [Fact]
    public async Task Create_ValidSessionGameBoard_CallsRepositoryAdd()
    {
        // Arrange
        var sgb = new GameBoard { Id = Guid.NewGuid(), SessionId = Guid.NewGuid(), Data = "New" };
        _mockRepository.Setup(r => r.Add(sgb)).Returns(Task.CompletedTask);

        // Act
        var result = await _service.Create(sgb);

        // Assert
        Assert.Equal(sgb, result);
        _mockRepository.Verify(r => r.Add(sgb), Times.Once);
    }

    [Fact]
    public async Task Update_ValidSessionGameBoard_CallsRepositoryUpdateAndNotifies()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sgb = new GameBoard { Id = id, SessionId = Guid.NewGuid(), Data = "Updated" };
        _mockRepository.Setup(r => r.Update(sgb)).Returns(Task.CompletedTask);
        // We don't need to mock GetById if Update is called directly with the object, 
        // unlike PlayerAction which calls GetById inside. 
        // But checking `GameBoardService.Update` implementation:
        // it calls `repository.Update(gameBoard)` directly.

        // Act
        await _service.Update(id, sgb);

        // Assert
        _mockRepository.Verify(r => r.Update(sgb), Times.Once);
        _mockClients.Verify(c => c.Group(sgb.SessionId.ToString()), Times.Once);
        _mockClientProxy.Verify(c => c.SendCoreAsync("GameBoardUpdated", It.IsAny<object[]>(), default), Times.Once);
    }

    [Fact]
    public async Task Delete_ValidId_CallsRepositoryDelete()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.Delete(id)).Returns(Task.CompletedTask);

        // Act
        await _service.Delete(id);

        // Assert
        _mockRepository.Verify(r => r.Delete(id), Times.Once);
    }
}
