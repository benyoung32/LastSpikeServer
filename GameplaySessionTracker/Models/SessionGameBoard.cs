using System;

namespace GameplaySessionTracker.Models
{
    public class SessionGameBoard
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public Guid BoardId { get; set; }
        public string Data { get; set; } = string.Empty;
    }
}
