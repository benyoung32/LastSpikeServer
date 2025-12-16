using System.Collections.Generic;

namespace GameplaySessionTracker.GameRules
{
    public enum TurnPhase
    {
        Start,
        SpaceOption,
        RouteSelect,
        End
    }

    public enum SpaceType
    {
        Go,
        Track,
        SettlerRents,
        Land,
        RoadbedCosts,
        Rebellion,
        EndOfTrack,
        LandClaims,
        SurveyFees,
        Scandal
    }

    public enum City
    {
        Calgary,
        Edmonton,
        Montreal,
        Regina,
        Saskatoon,
        Sudbury,
        Toronto,
        Vancouver,
        Winnipeg
    }

    public record Space(SpaceType Type, int Cost);

    public static class GameConstants
    {
        public static readonly int PlayerStartingMoney = 70000;
        public static readonly int LastSpikeBonus = 20000;
        public static readonly int CPRSubsidy = 5000;
        public static readonly List<Space> Spaces = new()
        {
            new Space(SpaceType.Go, 0),
            new Space(SpaceType.Track, 1000),
            new Space(SpaceType.SettlerRents, 1000),
            new Space(SpaceType.Land, 1000),
            new Space(SpaceType.RoadbedCosts, 1000),
            new Space(SpaceType.Track, 2000),
            new Space(SpaceType.Rebellion, 0),
            new Space(SpaceType.Land, 3000),
            new Space(SpaceType.Track, 4000),
            new Space(SpaceType.Land, 5000),
            new Space(SpaceType.EndOfTrack, 0),
            new Space(SpaceType.Track, 6000),
            new Space(SpaceType.LandClaims, 1000),
            new Space(SpaceType.Land, 7000),
            new Space(SpaceType.SurveyFees, 3000),
            new Space(SpaceType.Track, 8000),
            new Space(SpaceType.Scandal, 10000),
            new Space(SpaceType.Land, 9000),
            new Space(SpaceType.Track, 10000),
            new Space(SpaceType.Land, 12000)
        };

        public static readonly Dictionary<City, List<int>> CityValues = new()
        {
            [City.Calgary] = new List<int> { 0, 5000, 12000, 22000, 35000, 50000 },
            [City.Edmonton] = new List<int> { 0, 6000, 15000, 27000, 42000, 60000 },
            [City.Montreal] = new List<int> { 0, 10000, 25000, 45000, 70000, 100000 },
            [City.Regina] = new List<int> { 0, 7000, 17000, 32000, 50000, 70000 },
            [City.Saskatoon] = new List<int> { 0, 8000, 20000, 36000, 56000, 80000 },
            [City.Sudbury] = new List<int> { 0, 5000, 12000, 22000, 35000, 50000 },
            [City.Toronto] = new List<int> { 0, 6000, 15000, 27000, 42000, 60000 },
            [City.Vancouver] = new List<int> { 0, 9000, 22000, 40000, 63000, 90000 },
            [City.Winnipeg] = new List<int> { 0, 4000, 10000, 18000, 28000, 40000 }
        };

        public static readonly List<CityPair> ValidCityPairs =
        [
            new CityPair(City.Montreal, City.Toronto),
            new CityPair(City.Montreal, City.Sudbury),
            new CityPair(City.Toronto, City.Winnipeg),
            new CityPair(City.Toronto, City.Regina),
            new CityPair(City.Sudbury, City.Saskatoon),
            new CityPair(City.Sudbury, City.Winnipeg),
            new CityPair(City.Winnipeg, City.Calgary),
            new CityPair(City.Winnipeg, City.Edmonton),
            new CityPair(City.Regina, City.Calgary),
            new CityPair(City.Saskatoon, City.Edmonton),
            new CityPair(City.Calgary, City.Vancouver),
            new CityPair(City.Edmonton, City.Vancouver)
        ];
    }
}
