-- Drop existing tables in reverse order of dependencies
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'SessionPlayers')
BEGIN
    DROP TABLE SessionPlayers;
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Sessions')
BEGIN
    DROP TABLE Sessions;
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

-- Create LastSpike database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'LastSpike')
BEGIN
    CREATE DATABASE LastSpike;
END
GO

USE LastSpike;
GO

-- Create GameBoards table
CREATE TABLE GameBoards (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Description NVARCHAR(MAX) NOT NULL,
    Data NVARCHAR(MAX) NOT NULL
);
GO

-- Create Players table
CREATE TABLE Players (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Alias NVARCHAR(255) NOT NULL
);
GO

-- Create Sessions table
CREATE TABLE Sessions (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Description NVARCHAR(MAX) NOT NULL,
    BoardId UNIQUEIDENTIFIER NOT NULL,
    FOREIGN KEY (BoardId) REFERENCES GameBoards(Id)
);
GO

-- Create SessionPlayers junction table
CREATE TABLE SessionPlayers (
    SessionId UNIQUEIDENTIFIER NOT NULL,
    PlayerId UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY (SessionId, PlayerId),
    FOREIGN KEY (SessionId) REFERENCES Sessions(Id) ON DELETE CASCADE,
    FOREIGN KEY (PlayerId) REFERENCES Players(Id)
);
GO
