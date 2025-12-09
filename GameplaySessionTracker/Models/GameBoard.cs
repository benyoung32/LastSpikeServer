using System;

namespace GameplaySessionTracker.Models
{
    public class GameBoard
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        // TODO: add a mapping between player ID and a hash 
        // to validate that only the current player is sending actions
        public string Data { get; set; } = string.Empty;
    }
}
