using GameplaySessionTracker.Models;
using GameplaySessionTracker.Services;
using Microsoft.AspNetCore.Mvc;
namespace GameplaySessionTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionsController(
            ISessionService sessionService,
            IGameBoardService gameBoardService,
            IPlayerService playerService
    ) : ControllerBase
    {

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SessionData>>> GetAll()
        {
            return Ok(await sessionService.GetAll());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SessionData>> GetById(Guid id)
        {
            var session = await sessionService.GetById(id);
            if (session == null)
            {
                return NotFound();
            }
            return Ok(session);
        }



        [HttpPost]
        public async Task<ActionResult<SessionData>> Create(SessionData session)
        {
            // ignore user provided guids
            session.Id = Guid.NewGuid();
            session.BoardId = Guid.NewGuid();

            var createdSession = await sessionService.Create(session);

            var board = await gameBoardService.Create(
                    new GameBoard { Id = session.BoardId, SessionId = session.Id });

            return CreatedAtAction(nameof(GetById), new { id = createdSession.Id }, createdSession);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, SessionData session)
        {
            if (id != session.Id)
            {
                return BadRequest("ID mismatch");
            }

            var existing = await sessionService.GetById(id);
            if (existing == null)
            {
                return NotFound();
            }

            // Validate BoardId
            if (session.BoardId != Guid.Empty)
            {
                var board = await gameBoardService.GetById(session.BoardId);
                if (board == null)
                {
                    return BadRequest($"BoardId {session.BoardId} does not exist");
                }
            }

            // Validate PlayerIds
            foreach (var playerId in session.PlayerIds)
            {
                var player = playerService.GetById(playerId);
                if (player == null)
                {
                    return BadRequest($"PlayerId {playerId} does not exist");
                }
            }

            await sessionService.Update(session);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await sessionService.GetById(id);
            if (existing == null)
            {
                return NotFound();
            }

            await sessionService.Delete(id);
            // delete corresponding game board as well
            // this is unnecessar because of the ON DELETE CASCADE restraint on the database
            // if this restriction is changed, this will have to be changed
            // await gameBoardService.Delete(existing.BoardId);

            return NoContent();
        }

        [HttpPut("{id}/start_game")]
        public async Task<IActionResult> StartGame(Guid id, Guid playerId)
        {
            var session = await sessionService.GetById(id);
            if (session == null)
            {
                return NotFound();
            }

            if (playerId != session.PlayerIds[0])
            {
                return BadRequest("Only the host can start the game");
            }

            if (IsGameStarted(session))
            {
                return BadRequest("Game has already started");
            }

            await sessionService.StartGame(id, session);
            await gameBoardService.StartGame(session.BoardId, session.PlayerIds);

            return NoContent();
        }

        [HttpPost("{sessionId}/players/{playerId}")]
        public async Task<IActionResult> AddPlayer(Guid sessionId, Guid playerId)
        {
            // Check if session exists
            var session = await sessionService.GetById(sessionId);
            if (session == null)
            {
                return NotFound($"Session with ID {sessionId} not found");
            }

            // Check if player exists
            var player = playerService.GetById(playerId);
            if (player == null)
            {
                return NotFound($"Player with ID {playerId} not found");
            }

            // Check if player is already in the session
            if (session.PlayerIds.Contains(playerId))
            {
                return BadRequest($"Player {playerId} is already in session {sessionId}");
            }

            // Check if session has started
            if (IsGameStarted(session))
            {
                return BadRequest("Cannot add player to an in progress session");
            }

            await sessionService.AddPlayer(playerId, session);
            return Ok(new { message = $"Player {playerId} added to session {sessionId}" });
        }

        [HttpDelete("{sessionId}/players/{playerId}")]
        public async Task<IActionResult> RemovePlayer(Guid sessionId, Guid playerId)
        {
            // Validate session exists
            var session = await sessionService.GetById(sessionId);
            if (session == null)
            {
                return NotFound($"Session with ID {sessionId} not found");
            }

            // Check if player is in the session
            if (!session.PlayerIds.Contains(playerId))
            {
                return BadRequest($"Player {playerId} is not in session {sessionId}");
            }

            await sessionService.RemovePlayer(playerId, session);
            return Ok(new { message = $"Player {playerId} removed from session {sessionId}" });
        }

        [HttpGet("{sessionId}/players")]
        public async Task<ActionResult<IEnumerable<Player>>> GetSessionPlayers(Guid sessionId)
        {
            var session = await sessionService.GetById(sessionId);
            if (session == null)
            {
                return NotFound($"Session with ID {sessionId} not found");
            }

            var players = await sessionService.GetSessionPlayers(sessionId);
            return Ok(players);
        }

        private static bool IsGameStarted(SessionData session)
        {
            return session.StartTime != null;
        }
    }
}