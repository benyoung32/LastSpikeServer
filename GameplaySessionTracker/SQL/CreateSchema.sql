-- Drop existing tables in reverse order of dependencies
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'SessionPlayers') 
BEGIN
    DROP TABLE SessionPlayers;
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Players')
BEGIN
    DROP TABLE Players;
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'GameBoards')
BEGIN
    DROP TABLE GameBoards;
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Sessions')
BEGIN
    DROP TABLE Sessions;
END
GO


-- Create LastSpike database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TheLastSpike2')
BEGIN
    CREATE DATABASE LastSpike;
END
GO

USE TheLastSpike2;
GO

-- -- Create GameBoards table
-- CREATE TABLE GameBoards (
--     Id UNIQUEIDENTIFIER PRIMARY KEY,
--     Description NVARCHAR(MAX) NOT NULL,
--     Data NVARCHAR(MAX) NOT NULL
-- );
-- GO

CREATE TABLE Players (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
);
GO

CREATE TABLE Sessions (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Description NVARCHAR(255) NOT NULL,
    BoardId UNIQUEIDENTIFIER NOT NULL,
    StartTime DATETIME2 NULL,
    EndTime DATETIME2 NULL,
);
GO

CREATE TABLE SessionPlayers (
    SessionId UNIQUEIDENTIFIER NOT NULL,
    PlayerId UNIQUEIDENTIFIER NOT NULL,
    FOREIGN KEY (SessionId) REFERENCES Sessions(Id) ON DELETE CASCADE,
    FOREIGN KEY (PlayerId) REFERENCES Players(Id)
);
GO

CREATE TABLE GameBoards (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    SessionId UNIQUEIDENTIFIER NOT NULL,
    Data NVARCHAR(MAX) NOT NULL,
    FOREIGN KEY (SessionId) REFERENCES Sessions(Id) ON DELETE CASCADE
);
