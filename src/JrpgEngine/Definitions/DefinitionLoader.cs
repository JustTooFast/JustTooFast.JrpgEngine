// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using JustTooFast.JrpgEngine.Utils;

namespace JustTooFast.JrpgEngine.Definitions;

public static class DefinitionLoader
{
    public static DefinitionDatabase LoadAll(string dataRoot)
    {
        if (string.IsNullOrWhiteSpace(dataRoot))
        {
            throw new ArgumentException("Data root cannot be null or empty.", nameof(dataRoot));
        }

        if (!Directory.Exists(dataRoot))
        {
            throw new DirectoryNotFoundException($"Data root not found: {dataRoot}");
        }

        var gameConfigPath = Path.Combine(dataRoot, "game", "game_config.json");
        var mapsPath = Path.Combine(dataRoot, "maps");
        var charactersPath = Path.Combine(dataRoot, "characters");

        var gameConfig = JsonFile.Load<GameConfig>(gameConfigPath);
        var maps = LoadMapDefinitions(mapsPath);
        var characters = LoadCharacterDefinitions(charactersPath);

        ValidateGameConfig(gameConfig);
        ValidateMaps(maps);
        ValidateCharacters(characters);
        ValidateCrossReferences(gameConfig, maps, characters);

        return new DefinitionDatabase(gameConfig, maps, characters);
    }

    private static Dictionary<string, MapDef> LoadMapDefinitions(string mapsPath)
    {
        if (!Directory.Exists(mapsPath))
        {
            throw new DirectoryNotFoundException($"Maps folder not found: {mapsPath}");
        }

        var mapFiles = Directory.GetFiles(mapsPath, "*.json", SearchOption.TopDirectoryOnly);
        if (mapFiles.Length == 0)
        {
            throw new InvalidOperationException($"No map definition files found in: {mapsPath}");
        }

        var maps = new Dictionary<string, MapDef>(StringComparer.Ordinal);

        foreach (var filePath in mapFiles)
        {
            var mapDef = JsonFile.Load<MapDef>(filePath);

            if (string.IsNullOrWhiteSpace(mapDef.Id))
            {
                throw new InvalidOperationException($"Map definition in '{filePath}' is missing Id.");
            }

            if (!maps.TryAdd(mapDef.Id, mapDef))
            {
                throw new InvalidOperationException(
                    $"Duplicate map id '{mapDef.Id}' found in '{filePath}'.");
            }
        }

        return maps;
    }

    private static Dictionary<string, CharacterDef> LoadCharacterDefinitions(string charactersPath)
    {
        if (!Directory.Exists(charactersPath))
        {
            throw new DirectoryNotFoundException($"Characters folder not found: {charactersPath}");
        }

        var characterFiles = Directory.GetFiles(charactersPath, "*.json", SearchOption.TopDirectoryOnly);
        if (characterFiles.Length == 0)
        {
            throw new InvalidOperationException($"No character definition files found in: {charactersPath}");
        }

        var characters = new Dictionary<string, CharacterDef>(StringComparer.Ordinal);

        foreach (var filePath in characterFiles)
        {
            var characterDef = JsonFile.Load<CharacterDef>(filePath);

            if (string.IsNullOrWhiteSpace(characterDef.Id))
            {
                throw new InvalidOperationException($"Character definition in '{filePath}' is missing Id.");
            }

            if (!characters.TryAdd(characterDef.Id, characterDef))
            {
                throw new InvalidOperationException(
                    $"Duplicate character id '{characterDef.Id}' found in '{filePath}'.");
            }
        }

        return characters;
    }

    private static void ValidateGameConfig(GameConfig gameConfig)
    {
        if (string.IsNullOrWhiteSpace(gameConfig.StartingMapId))
        {
            throw new InvalidOperationException("GameConfig.StartingMapId is required.");
        }

        if (string.IsNullOrWhiteSpace(gameConfig.StartingFacing))
        {
            throw new InvalidOperationException("GameConfig.StartingFacing is required.");
        }

        if (gameConfig.StartingPartyMemberIds.Count == 0)
        {
            throw new InvalidOperationException(
                "GameConfig must define at least one StartingPartyMemberId.");
        }
    }

    private static void ValidateMaps(IReadOnlyDictionary<string, MapDef> maps)
    {
        foreach (var (mapId, mapDef) in maps)
        {
            if (mapDef.Width <= 0)
            {
                throw new InvalidOperationException($"Map '{mapId}' must have Width > 0.");
            }

            if (mapDef.Height <= 0)
            {
                throw new InvalidOperationException($"Map '{mapId}' must have Height > 0.");
            }

            if (mapDef.TileSize <= 0)
            {
                throw new InvalidOperationException($"Map '{mapId}' must have TileSize > 0.");
            }

            foreach (var blockedTile in mapDef.BlockedTiles)
            {
                if (blockedTile.X < 0 || blockedTile.X >= mapDef.Width ||
                    blockedTile.Y < 0 || blockedTile.Y >= mapDef.Height)
                {
                    throw new InvalidOperationException(
                        $"Map '{mapId}' contains out-of-bounds blocked tile ({blockedTile.X}, {blockedTile.Y}).");
                }
            }
        }
    }

    private static void ValidateCharacters(IReadOnlyDictionary<string, CharacterDef> characters)
    {
        foreach (var (characterId, characterDef) in characters)
        {
            if (string.IsNullOrWhiteSpace(characterDef.Name))
            {
                throw new InvalidOperationException(
                    $"Character '{characterId}' must have a non-empty Name.");
            }
        }
    }

    private static void ValidateCrossReferences(
        GameConfig gameConfig,
        IReadOnlyDictionary<string, MapDef> maps,
        IReadOnlyDictionary<string, CharacterDef> characters)
    {
        if (!maps.TryGetValue(gameConfig.StartingMapId, out var startingMap))
        {
            throw new InvalidOperationException(
                $"GameConfig.StartingMapId '{gameConfig.StartingMapId}' does not exist.");
        }

        if (gameConfig.StartingPlayerTileX < 0 || gameConfig.StartingPlayerTileX >= startingMap.Width ||
            gameConfig.StartingPlayerTileY < 0 || gameConfig.StartingPlayerTileY >= startingMap.Height)
        {
            throw new InvalidOperationException(
                $"Starting player tile ({gameConfig.StartingPlayerTileX}, {gameConfig.StartingPlayerTileY}) " +
                $"is out of bounds for map '{startingMap.Id}'.");
        }

        foreach (var blockedTile in startingMap.BlockedTiles)
        {
            if (blockedTile.X == gameConfig.StartingPlayerTileX &&
                blockedTile.Y == gameConfig.StartingPlayerTileY)
            {
                throw new InvalidOperationException(
                    $"Starting player tile ({gameConfig.StartingPlayerTileX}, {gameConfig.StartingPlayerTileY}) " +
                    $"is blocked on map '{startingMap.Id}'.");
            }
        }

        var seenPartyIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var characterId in gameConfig.StartingPartyMemberIds)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                throw new InvalidOperationException(
                    "GameConfig.StartingPartyMemberIds cannot contain null or empty ids.");
            }

            if (!seenPartyIds.Add(characterId))
            {
                throw new InvalidOperationException(
                    $"GameConfig.StartingPartyMemberIds contains duplicate id '{characterId}'.");
            }

            if (!characters.ContainsKey(characterId))
            {
                throw new InvalidOperationException(
                    $"GameConfig references missing starting party character '{characterId}'.");
            }
        }
    }
}