# LastSpikeServer

The backend server for the "Last Spike" board game, built with ASP.NET Core. This application manages game sessions, player states, and the core game logic through a centralized rule engine accessed via a REST API. [A browser client is available here](https://github.com/benyoung32/LastSpikeClient).

## Inspiration

The goal behind project was to recreate one of my favorite board games, The Last Spike 1976, inside a web application so that I could play the game with friends from far away. The classic Canadian board game is a monopoly-type game where players compete to hoard properties and money while building the transcontinental railroad across Canada. The ultimate goal of each player is to have the most money when the last spike of the rail connecting Vancouver and Montreal is driven.

## Setup

The project is built with .NET 9.0

The project can be run locally with `dotnet run` from the GameplaySessionTracker directory. A SQL database is required, which can be initialized using the CreateSchema.sql file in the SQL folder. The connection string can be set inside the appsettings.json file.

The project is currently deployed on Azure and the frontend is hosted at [https://last-spike-client.vercel.app/](https://last-spike-client.vercel.app/)

## Client/Server Communication

Clients first get a PlayerID from the Player service, then join a session using that PlayerID. Clients subscribe to a SignalR hub to receive notifications about the game state. SignalR is only used to notify clients of changes, while the client makes API requests to retrieve the game state. Valid actions are sent to clients as part of the game state. Clients make a PUT request with their selected action, which the server validates and executes. When a new game state is pushed, a notification is sent to all clients in the session.

## Controllers

 The API is organized into three main controllers:

### 1. `SessionsController`

Manages the lifecycle of game sessions.

- **Responsibilities**:

  - Creating new game sessions.
  - Retrieving session details.
  - Adding/removing players from the lobby.
  - Starting the game (transitioning from lobby to active game state).

### 2. `PlayersController`

Handles player management.

- **Responsibilities**:

  - Creating, retrieving, updating, and deleting player profiles.
  - Used primarily for initial player setup before joining sessions.
  - The created PlayerID GUID is the primary key used by the client to identify the player while making subsequent API requests.

### 3. `GameBoardsController`

Manages gameplay functions.

- **Responsibilities**:

  - Retrieving the current `GameState` for a specific session.
  - Handling player actions (Rolling dice, Buying property, Placing track, etc.).
  - Handling trade offers and responses.

## Rule Engine

The `RuleEngine` (`GameRules/RuleEngine.cs`) is the core of the application. It is a static class that implements a state-transition model. It takes an existing `GameState` and an action, and returns a new `GameState`. The methods contained here describe all of the actions and events that can occur in the game. Defining record types for each component of the game state, as well as many enums, gives type safety and enables the use of pattern matching to handle different types of actions.

The RuleEngine contains all of the hard rules of the game, as taken from the board game's manual. All changes to state are made here, while the GameBoardService handles which method to call. GameBoardController validates client API requests.

All of the game logic is replicated inside the client as well for presentation purposes, however the server is the sole authorative source of truth for the game state.

### Game State

All game state information is stored in the `GameState` record, which itself is made up of many smaller records. Each GameState object is a complete picture of the game at a given moment in time. I chose to use immutable records to handle the game state, and manipulating/modifying state inside static methods since all game states will need to be serialized to JSON for storage in the database and transmission to clients.

The `GameState` is a C# `record` containing all information about the board:

- **`Players`**: An `OrderedDictionary` defining turn order and holding individual `PlayerState`.
- **`Routes`**: Tracks the number of tracks laid between different city pairs.
- **`Properties`**: The deck of available properties and who owns specific ones.
- **`TurnPhase`**: A state machine enum (`Start`, `SpaceOption`, `RouteSelect`, `End`) to help determine what actions are available to the player and control the start and ends of turns.

### Turn Logic

A player's turn flows through several phases:

- **Start**: The player can roll the dice or offer trades.
- **SpaceOption**: Depending on the space type (Land, Track, etc.), the player is presented with choices (e.g., Buy Property, Place Track).
- **RouteSelect**: Some actions require choosing a route to perform them on.
- **End**: The turn moves to the next player.

## Repository Setup

The project uses a Repository pattern with **Dapper** for lightweight and efficient data access to a SQL Server database.

- **`ISessionRepository`**: Handles storage for `SessionData` and the many-to-many relationship regarding which players are in which session (`SessionPlayers`).
- **`IPlayerRepository`**: Manages `Player` entity storage.
- **`IGameBoardRepository`**: Stores the serialized `GameState` JSON blobs within `GameBoard` entities.

Repositories are registered as **Singletons** in `Program.cs` and injected into services (`SessionService`, `PlayerService`, `GameBoardService`). Connection strings are environment-aware, switching between generic "DefaultConnection" and "LocalConnection" for development.
