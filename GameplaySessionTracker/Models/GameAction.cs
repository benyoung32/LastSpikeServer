using System.Text.Json.Serialization;
using GameplaySessionTracker.GameRules;

namespace GameplaySessionTracker.Models
{
    public enum ActionType
    {
        /*         
        Go,
        Track,
        SettlerRents,
        Land,
        RoadbedCosts,
        Rebellion,
        EndOfTrack,
        LandClaims,
        SurveyFees
        */
        Move,
        Ok,
        Pass,
        Rebellion,
        TradeOffer,
        PlaceTrack,
        Roll,
        Buy,
        AcceptTradeOffer
    }

    public class GameAction
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required ActionType Type { get; set; }
        public required Guid PlayerId { get; set; }
        // TODO: add trade data 

        public CityPair? Target { get; set; }
    }
}