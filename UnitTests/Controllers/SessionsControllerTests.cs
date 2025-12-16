using GameplaySessionTracker.Controllers;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
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
        var session = new SessionData { Id = Guid.NewGuid(), PlayerIds = new List<Guid>() };
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
    public async Task Create_Valid_ReturnsCreatedAndCreatesBoard()
    {
        var session = new SessionData { PlayerIds = new List<Guid>() };
        var createdSession = new SessionData { Id = Guid.NewGuid(), BoardId = Guid.NewGuid(), PlayerIds = new List<Guid>() };

        _mockService.Setup(s => s.Create(It.IsAny<SessionData>())).ReturnsAsync(createdSession);
        _mockBoardService.Setup(s => s.Create(It.IsAny<GameBoard>())).ReturnsAsync(new GameBoard());

        var result = await _controller.Create(session);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(createdSession, createdResult.Value);
        _mockService.Verify(s => s.Create(It.IsAny<SessionData>()), Times.Once);
        _mockBoardService.Verify(s => s.Create(It.IsAny<GameBoard>()), Times.Once);
    }

    [Fact]
    public async Task AddPlayer_Valid_CallsService()
    {
        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var session = new SessionData { Id = sessionId, PlayerIds = new List<Guid>() };

        _mockService.Setup(s => s.GetById(sessionId)).ReturnsAsync(session);
        _mockPlayerService.Setup(s => s.GetById(playerId)).Returns(new Player { Id = playerId });

        var result = await _controller.AddPlayer(sessionId, playerId);

        Assert.IsType<OkObjectResult>(result);
        _mockService.Verify(s => s.AddPlayer(playerId, session), Times.Once);
    }

    [Fact]
    public async Task AddPlayer_SessionNotFound_ReturnsNotFound()
    {
        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        _mockService.Setup(s => s.GetById(sessionId)).ReturnsAsync((SessionData?)null);

        var result = await _controller.AddPlayer(sessionId, playerId);

        // Assert string match for message if needed, or just type
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(sessionId.ToString(), notFoundResult.Value?.ToString());
    }

    [Fact]
    public async Task AddPlayer_PlayerNotFound_ReturnsNotFound()
    {
        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var session = new SessionData { Id = sessionId, PlayerIds = new List<Guid>() };

        _mockService.Setup(s => s.GetById(sessionId)).ReturnsAsync(session);
        _mockPlayerService.Setup(s => s.GetById(playerId)).Returns((Player?)null);

        var result = await _controller.AddPlayer(sessionId, playerId);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(playerId.ToString(), notFoundResult.Value?.ToString());
    }

    [Fact]
    public async Task AddPlayer_PlayerAlreadyInSession_ReturnsBadRequest()
    {
        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var session = new SessionData { Id = sessionId, PlayerIds = new List<Guid> { playerId } };

        _mockService.Setup(s => s.GetById(sessionId)).ReturnsAsync(session);
        _mockPlayerService.Setup(s => s.GetById(playerId)).Returns(new Player { Id = playerId });

        var result = await _controller.AddPlayer(sessionId, playerId);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task RemovePlayer_Valid_CallsService()
    {
        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var session = new SessionData { Id = sessionId, PlayerIds = new List<Guid> { playerId } };

        _mockService.Setup(s => s.GetById(sessionId)).ReturnsAsync(session);

        var result = await _controller.RemovePlayer(sessionId, playerId);

        Assert.IsType<OkObjectResult>(result);
        _mockService.Verify(s => s.RemovePlayer(playerId, session), Times.Once);
    }

    [Fact]
    public async Task RemovePlayer_PlayerNotInSession_ReturnsBadRequest()
    {
        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var session = new SessionData { Id = sessionId, PlayerIds = new List<Guid>() }; // Empty

        _mockService.Setup(s => s.GetById(sessionId)).ReturnsAsync(session);

        var result = await _controller.RemovePlayer(sessionId, playerId);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_Valid_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        var session = new SessionData { Id = id, BoardId = Guid.NewGuid(), PlayerIds = new List<Guid>() };

        _mockService.Setup(s => s.GetById(id)).ReturnsAsync(session);
        _mockBoardService.Setup(s => s.GetById(session.BoardId)).ReturnsAsync(new GameBoard { Id = session.BoardId });

        var result = await _controller.Update(id, session);
        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.Update(session), Times.Once);
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        var id = Guid.NewGuid();
        var session = new SessionData { Id = Guid.NewGuid(), PlayerIds = new List<Guid>() };
        var result = await _controller.Update(id, session);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Delete_Existing_ReturnsNoContentAndDeletesBoard()
    {
        var id = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var session = new SessionData { Id = id, BoardId = boardId, PlayerIds = new List<Guid>() };

        _mockService.Setup(s => s.GetById(id)).ReturnsAsync(session);

        var result = await _controller.Delete(id);

        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.Delete(id), Times.Once);
        _mockBoardService.Verify(s => s.Delete(boardId), Times.Once);
    }

    [Fact]
    public async Task StartGame_HostStartsGame_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        var hostId = Guid.NewGuid();
        var session = new SessionData { Id = id, PlayerIds = new List<Guid> { hostId }, BoardId = Guid.NewGuid() };

        _mockService.Setup(s => s.GetById(id)).ReturnsAsync(session);

        var result = await _controller.StartGame(id, hostId);

        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.StartGame(id, session), Times.Once);
        _mockBoardService.Verify(s => s.StartGame(session.BoardId, session.PlayerIds), Times.Once);
    }

    [Fact]
    public async Task StartGame_NonHostStartsGame_ReturnsBadRequest()
    {
        var id = Guid.NewGuid();
        var hostId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var session = new SessionData { Id = id, PlayerIds = new List<Guid> { hostId, otherId } };

        _mockService.Setup(s => s.GetById(id)).ReturnsAsync(session);

        var result = await _controller.StartGame(id, otherId);

        // Expect BadRequest as only host (index 0) can start
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
