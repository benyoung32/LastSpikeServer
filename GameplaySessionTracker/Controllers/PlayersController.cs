using System;
using System.Collections.Generic;
using System.Linq;
using GameplaySessionTracker.Models;
using GameplaySessionTracker.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameplaySessionTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController(IPlayerService playerService) : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<Player>> GetAll()
        {
            return Ok(playerService.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<Player> GetById(Guid id)
        {
            var player = playerService.GetById(id);
            if (player == null)
            {
                return NotFound();
            }
            return Ok(player);
        }

        [HttpPost]
        public ActionResult<Player> Create(Player player)
        {
            player.Id = Guid.NewGuid();
            var createdPlayer = playerService.Create(player);
            return CreatedAtAction(nameof(GetById), new { id = createdPlayer.Id }, createdPlayer);
        }

        [HttpPut("{id}")]
        public IActionResult Update(Guid id, Player player)
        {
            if (id != player.Id && player.Id != Guid.Empty)
            {
                return BadRequest("ID mismatch");
            }

            var existing = playerService.GetById(id);
            if (existing == null)
            {
                return NotFound();
            }

            playerService.Update(id, player);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var existing = playerService.GetById(id);
            if (existing == null)
            {
                return NotFound();
            }

            playerService.Delete(id);
            return NoContent();
        }

    }
}
