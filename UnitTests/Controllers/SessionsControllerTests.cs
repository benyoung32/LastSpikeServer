using GameplaySessionTracker.Controllers;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using System.Threading.Tasks;

namespace GameplaySessionTracker.Tests.Controllers;

public class SessionsControllerTests
{
    private readonly Mock<ISessionService> _mockService;
    private readonly Mock<IGameBoardService> _mockBoardService;
    private readonly Mock<IPlayerService> _mockPlayerService;
    private readonly SessionsController _controller;

    public SessionsControllerTests()
    {
        _mockService = new Mock<ISessionService>();
        _mockBoardService = new Mock<IGameBoardService>();
        _mockPlayerService = new Mock<IPlayerService>();
        _controller = new SessionsController(_mockService.Object, _mockBoardService.Object, _mockPlayerService.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _mockService.Setup(s => s.GetAll()).ReturnsAsync(new List<SessionData>());
        var result = await _controller.GetAll();
        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetById_Existing_ReturnsOk()
    {
        var session = new SessionData { Id = Guid.NewGuid() };
        _mockService.Setup(s => s.GetById(session.Id)).ReturnsAsync(session);
        var result = await _controller.GetById(session.Id);
        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetById_NonExistent_ReturnsNotFound()
    {
        _mockService.Setup(s => s.GetById(It.IsAny<Guid>())).ReturnsAsync((SessionData?)null);
        var result = await _controller.GetById(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result.Result);
    }





    [Fact]
    public async Task Create_Valid_ReturnsCreated()
    {
        var session = new SessionData { BoardId = Guid.NewGuid(), PlayerIds = new List<Guid>() };
        _mockBoardService.Setup(s => s.GetById(session.BoardId)).ReturnsAsync(new GameBoard { Id = session.BoardId });
        _mockService.Setup(s => s.Create(It.IsAny<SessionData>())).ReturnsAsync(new SessionData { Id = Guid.NewGuid() });
        var result = await _controller.Create(session);
        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task AddPlayerToSession_FirstPlayer_SetsStartTime()
    {
        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var session = new SessionData { Id = sessionId, PlayerIds = new List<Guid>() };
        _mockService.Setup(s => s.GetById(sessionId)).ReturnsAsync(session);
        _mockPlayerService.Setup(s => s.GetById(playerId)).Returns(new Player { Id = playerId });

        var result = await _controller.AddPlayerToSession(sessionId, playerId);

        Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(session.StartTime);
        _mockService.Verify(s => s.Update(sessionId, session), Times.Once);
    }

    [Fact]
    public async Task RemovePlayerFromSession_LastPlayer_SetsEndTime()
    {
        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var session = new SessionData { Id = sessionId, PlayerIds = new List<Guid> { playerId } };
        _mockService.Setup(s => s.GetById(sessionId)).ReturnsAsync(session);

        var result = await _controller.RemovePlayerFromSession(sessionId, playerId);

        Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(session.EndTime);
        _mockService.Verify(s => s.Update(sessionId, session), Times.Once);
    }

    [Fact]
    public async Task Update_Valid_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        var session = new SessionData { Id = id, BoardId = Guid.NewGuid() };
        _mockService.Setup(s => s.GetById(id)).ReturnsAsync(session);
        _mockBoardService.Setup(s => s.GetById(session.BoardId)).ReturnsAsync(new GameBoard { Id = session.BoardId });
        var result = await _controller.Update(id, session);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_NonExistent_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        var session = new SessionData { Id = id };
        _mockService.Setup(s => s.GetById(id)).ReturnsAsync((SessionData?)null);
        var result = await _controller.Update(id, session);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        var id = Guid.NewGuid();
        var session = new SessionData { Id = Guid.NewGuid() };
        var result = await _controller.Update(id, session);
        Assert.IsType<BadRequestObjectResult>(result);
    }



    [Fact]
    public async Task Update_InvalidBoardId_ReturnsBadRequest()
    {
        var id = Guid.NewGuid();
        var session = new SessionData { Id = id, BoardId = Guid.NewGuid() };
        _mockService.Setup(s => s.GetById(id)).ReturnsAsync(session);
        _mockBoardService.Setup(s => s.GetById(session.BoardId)).ReturnsAsync((GameBoard?)null);
        var result = await _controller.Update(id, session);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_InvalidPlayerId_ReturnsBadRequest()
    {
        var id = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var session = new SessionData { Id = id, PlayerIds = new List<Guid> { playerId } };
        _mockService.Setup(s => s.GetById(id)).ReturnsAsync(session);
        _mockPlayerService.Setup(s => s.GetById(playerId)).Returns((Player?)null);
        var result = await _controller.Update(id, session);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Delete_NonExistent_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetById(id)).ReturnsAsync((SessionData?)null);
        var result = await _controller.Delete(id);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AddPlayerToSession_SessionNotFound_ReturnsNotFound()
    {
        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        _mockService.Setup(s => s.GetById(sessionId)).ReturnsAsync((SessionData?)null);
        var result = await _controller.AddPlayerToSession(sessionId, playerId);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task AddPlayerToSession_PlayerNotFound_ReturnsNotFound()
    {
        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var session = new SessionData { Id = sessionId, PlayerIds = new List<Guid>() };
        _mockService.Setup(s => s.GetById(sessionId)).ReturnsAsync(session);
        _mockPlayerService.Setup(s => s.GetById(playerId)).Returns((Player?)null);
        var result = await _controller.AddPlayerToSession(sessionId, playerId);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task AddPlayerToSession_PlayerAlreadyInSession_ReturnsBadRequest()
    {
        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var session = new SessionData { Id = sessionId, PlayerIds = new List<Guid> { playerId } };
        _mockService.Setup(s => s.GetById(sessionId)).ReturnsAsync(session);
        _mockPlayerService.Setup(s => s.GetById(playerId)).Returns(new Player { Id = playerId });
        var result = await _controller.AddPlayerToSession(sessionId, playerId);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AddPlayerToSession_SubsequentPlayer_DoesNotSetStartTime()
    {
        var sessionId = Guid.NewGuid();
        var playerId1 = Guid.NewGuid();
        var playerId2 = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddHours(-1);
        var session = new SessionData { Id = sessionId, PlayerIds = new List<Guid> { playerId1 }, StartTime = startTime };
        _mockService.Setup(s => s.GetById(sessionId)).ReturnsAsync(session);
        _mockPlayerService.Setup(s => s.GetById(playerId2)).Returns(new Player { Id = playerId2 });

        var result = await _controller.AddPlayerToSession(sessionId, playerId2);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(startTime, session.StartTime); // Should not change
    }

    [Fact]
    public async Task RemovePlayerFromSession_SessionNotFound_ReturnsNotFound()
    {
        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        _mockService.Setup(s => s.GetById(sessionId)).ReturnsAsync((SessionData?)null);
        var result = await _controller.RemovePlayerFromSession(sessionId, playerId);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task RemovePlayerFromSession_PlayerNotInSession_ReturnsBadRequest()
    {
        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var session = new SessionData { Id = sessionId, PlayerIds = new List<Guid>() };
        _mockService.Setup(s => s.GetById(sessionId)).ReturnsAsync(session);
        var result = await _controller.RemovePlayerFromSession(sessionId, playerId);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task RemovePlayerFromSession_NotLastPlayer_DoesNotSetEndTime()
    {
        var sessionId = Guid.NewGuid();
        var playerId1 = Guid.NewGuid();
        var playerId2 = Guid.NewGuid();
        var session = new SessionData { Id = sessionId, PlayerIds = new List<Guid> { playerId1, playerId2 } };
        _mockService.Setup(s => s.GetById(sessionId)).ReturnsAsync(session);

        var result = await _controller.RemovePlayerFromSession(sessionId, playerId1);

        Assert.IsType<OkObjectResult>(result);
        Assert.Null(session.EndTime); // Should not be set
    }

    [Fact]
    public async Task Delete_Existing_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetById(id)).ReturnsAsync(new SessionData { Id = id });
        var result = await _controller.Delete(id);
        Assert.IsType<NoContentResult>(result);
    }
}
