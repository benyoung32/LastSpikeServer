using System;
using System.Collections.Generic;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameplaySessionTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameBoardsController : ControllerBase
    {
        private readonly IGameBoardService _gameBoardService;

        public GameBoardsController(IGameBoardService gameBoardService)
        {
            _gameBoardService = gameBoardService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<GameBoard>> GetAll()
        {
            return Ok(_gameBoardService.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<GameBoard> GetById(Guid id)
        {
            var gameBoard = _gameBoardService.GetById(id);
            if (gameBoard == null)
            {
                return NotFound();
            }
            return Ok(gameBoard);
        }

        [HttpPost]
        public ActionResult<GameBoard> Create(GameBoard gameBoard)
        {
            gameBoard.Id = Guid.NewGuid();
            var createdGameBoard = _gameBoardService.Create(gameBoard);
            return CreatedAtAction(nameof(GetById), new { id = createdGameBoard.Id }, createdGameBoard);
        }

        [HttpPut("{id}")]
        public IActionResult Update(Guid id, GameBoard gameBoard)
        {
            if (id != gameBoard.Id && gameBoard.Id != Guid.Empty)
            {
                return BadRequest("ID mismatch");
            }

            var existing = _gameBoardService.GetById(id);
            if (existing == null)
            {
                return NotFound();
            }

            _gameBoardService.Update(id, gameBoard);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var existing = _gameBoardService.GetById(id);
            if (existing == null)
            {
                return NotFound();
            }

            _gameBoardService.Delete(id);
            return NoContent();
        }
    }
}
