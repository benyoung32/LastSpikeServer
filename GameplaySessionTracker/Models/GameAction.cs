using System.Text.Json.Serialization;

namespace GameplaySessionTracker.Models
{
    public enum ActionType
    {
        Roll,
        Accept,
        Pass
    }

    public class GameAction
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ActionType Name { get; set; }
        public Guid PlayerId { get; set; }

        public string Data { get; set; } = string.Empty;
    }
}