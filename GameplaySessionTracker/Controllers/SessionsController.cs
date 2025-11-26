using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameplaySessionTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionsController : ControllerBase
    {
        private readonly ISessionService _sessionService;
        private readonly IGameBoardService _gameBoardService;
        private readonly IPlayerService _playerService;

        public SessionsController(ISessionService sessionService, IGameBoardService gameBoardService, IPlayerService playerService)
        {
            _sessionService = sessionService;
            _gameBoardService = gameBoardService;
            _playerService = playerService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<SessionData>> GetAll()
        {
            return Ok(_sessionService.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<SessionData> GetById(Guid id)
        {
            var session = _sessionService.GetById(id);
            if (session == null)
            {
                return NotFound();
            }
            return Ok(session);
        }

        [HttpPost]
        public ActionResult<SessionData> Create(SessionData session)
        {
            session.Id = Guid.NewGuid();

            // Validate BoardId
            if (session.BoardId != Guid.Empty)
            {
                var board = _gameBoardService.GetById(session.BoardId);
                if (board == null)
                {
                    return BadRequest($"BoardId {session.BoardId} does not exist");
                }
            }

            // Validate PlayerIds
            foreach (var playerId in session.PlayerIds)
            {
                var player = _playerService.GetById(playerId);
                if (player == null)
                {
                    return BadRequest($"PlayerId {playerId} does not exist");
                }
            }

            var createdSession = _sessionService.Create(session);
            return CreatedAtAction(nameof(GetById), new { id = createdSession.Id }, createdSession);
        }

        [HttpPut("{id}")]
        public IActionResult Update(Guid id, SessionData session)
        {
            if (id != session.Id && session.Id != Guid.Empty)
            {
                return BadRequest("ID mismatch");
            }

            var existing = _sessionService.GetById(id);
            if (existing == null)
            {
                return NotFound();
            }

            // Validate BoardId
            if (session.BoardId != Guid.Empty)
            {
                var board = _gameBoardService.GetById(session.BoardId);
                if (board == null)
                {
                    return BadRequest($"BoardId {session.BoardId} does not exist");
                }
            }

            // Validate PlayerIds
            foreach (var playerId in session.PlayerIds)
            {
                var player = _playerService.GetById(playerId);
                if (player == null)
                {
                    return BadRequest($"PlayerId {playerId} does not exist");
                }
            }

            _sessionService.Update(id, session);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var existing = _sessionService.GetById(id);
            if (existing == null)
            {
                return NotFound();
            }

            _sessionService.Delete(id);
            return NoContent();
        }

        [HttpPost("{sessionId}/players/{playerId}")]
        public IActionResult AddPlayerToSession(Guid sessionId, Guid playerId)
        {
            // Validate session exists
            var session = _sessionService.GetById(sessionId);
            if (session == null)
            {
                return NotFound($"Session with ID {sessionId} not found");
            }

            // Validate player exists
            var player = _playerService.GetById(playerId);
            if (player == null)
            {
                return NotFound($"Player with ID {playerId} not found");
            }

            // Check if player is already in the session
            if (session.PlayerIds.Contains(playerId))
            {
                return BadRequest($"Player {playerId} is already in session {sessionId}");
            }

            // Add player to session
            session.PlayerIds.Add(playerId);
            _sessionService.Update(sessionId, session);

            return Ok(new { message = $"Player {playerId} added to session {sessionId}" });
        }

        [HttpDelete("{sessionId}/players/{playerId}")]
        public IActionResult RemovePlayerFromSession(Guid sessionId, Guid playerId)
        {
            // Validate session exists
            var session = _sessionService.GetById(sessionId);
            if (session == null)
            {
                return NotFound($"Session with ID {sessionId} not found");
            }

            // Check if player is in the session
            if (!session.PlayerIds.Contains(playerId))
            {
                return BadRequest($"Player {playerId} is not in session {sessionId}");
            }

            // Remove player from session
            session.PlayerIds.Remove(playerId);
            _sessionService.Update(sessionId, session);

            return Ok(new { message = $"Player {playerId} removed from session {sessionId}" });
        }
    }
}
