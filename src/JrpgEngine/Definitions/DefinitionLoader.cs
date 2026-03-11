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
        var dialoguesPath = Path.Combine(dataRoot, "dialogues");
        var interactionsPath = Path.Combine(dataRoot, "interactions");
        var itemsPath = Path.Combine(dataRoot, "items");
        var enemiesPath = Path.Combine(dataRoot, "enemies");
        var encountersPath = Path.Combine(dataRoot, "encounters");
        var encounterTablesPath = Path.Combine(dataRoot, "encounter_tables");

        var gameConfig = JsonFile.Load<GameConfig>(gameConfigPath);
        var maps = LoadMapDefinitions(mapsPath);
        var characters = LoadCharacterDefinitions(charactersPath);
        var dialogues = LoadDialogueDefinitions(dialoguesPath);
        var interactions = LoadInteractionDefinitions(interactionsPath);
        var items = LoadItemDefinitions(itemsPath);
        var enemies = LoadEnemyDefinitions(enemiesPath);
        var encounters = LoadEncounterDefinitions(encountersPath);
        var encounterTables = LoadEncounterTableDefinitions(encounterTablesPath);

        ValidateGameConfig(gameConfig);
        ValidateMaps(maps);
        ValidateCharacters(characters);
        ValidateDialogues(dialogues);
        ValidateInteractions(interactions);
        ValidateItems(items);
        ValidateEnemies(enemies);
        ValidateEncounters(encounters);
        ValidateEncounterTables(encounterTables);
        ValidateCrossReferences(
            gameConfig,
            maps,
            characters,
            dialogues,
            interactions,
            items,
            enemies,
            encounters,
            encounterTables);

        return new DefinitionDatabase(
            gameConfig,
            maps,
            characters,
            dialogues,
            interactions,
            items,
            enemies,
            encounters,
            encounterTables);
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

    private static Dictionary<string, DialogueDef> LoadDialogueDefinitions(string dialoguesPath)
    {
        if (!Directory.Exists(dialoguesPath))
        {
            throw new DirectoryNotFoundException($"Dialogues folder not found: {dialoguesPath}");
        }

        var dialogueFiles = Directory.GetFiles(dialoguesPath, "*.json", SearchOption.TopDirectoryOnly);
        if (dialogueFiles.Length == 0)
        {
            throw new InvalidOperationException($"No dialogue definition files found in: {dialoguesPath}");
        }

        var dialogues = new Dictionary<string, DialogueDef>(StringComparer.Ordinal);

        foreach (var filePath in dialogueFiles)
        {
            var dialogueDef = JsonFile.Load<DialogueDef>(filePath);

            if (string.IsNullOrWhiteSpace(dialogueDef.Id))
            {
                throw new InvalidOperationException($"Dialogue definition in '{filePath}' is missing Id.");
            }

            if (!dialogues.TryAdd(dialogueDef.Id, dialogueDef))
            {
                throw new InvalidOperationException(
                    $"Duplicate dialogue id '{dialogueDef.Id}' found in '{filePath}'.");
            }
        }

        return dialogues;
    }

    private static Dictionary<string, InteractionDef> LoadInteractionDefinitions(string interactionsPath)
    {
        if (!Directory.Exists(interactionsPath))
        {
            throw new DirectoryNotFoundException($"Interactions folder not found: {interactionsPath}");
        }

        var interactionFiles = Directory.GetFiles(interactionsPath, "*.json", SearchOption.TopDirectoryOnly);
        if (interactionFiles.Length == 0)
        {
            throw new InvalidOperationException($"No interaction definition files found in: {interactionsPath}");
        }

        var interactions = new Dictionary<string, InteractionDef>(StringComparer.Ordinal);

        foreach (var filePath in interactionFiles)
        {
            var interactionDef = JsonFile.Load<InteractionDef>(filePath);

            if (string.IsNullOrWhiteSpace(interactionDef.Id))
            {
                throw new InvalidOperationException($"Interaction definition in '{filePath}' is missing Id.");
            }

            if (!interactions.TryAdd(interactionDef.Id, interactionDef))
            {
                throw new InvalidOperationException(
                    $"Duplicate interaction id '{interactionDef.Id}' found in '{filePath}'.");
            }
        }

        return interactions;
    }

    private static Dictionary<string, ItemDef> LoadItemDefinitions(string itemsPath)
    {
        if (!Directory.Exists(itemsPath))
        {
            throw new DirectoryNotFoundException($"Items folder not found: {itemsPath}");
        }

        var itemFiles = Directory.GetFiles(itemsPath, "*.json", SearchOption.TopDirectoryOnly);
        if (itemFiles.Length == 0)
        {
            throw new InvalidOperationException($"No item definition files found in: {itemsPath}");
        }

        var items = new Dictionary<string, ItemDef>(StringComparer.Ordinal);

        foreach (var filePath in itemFiles)
        {
            var itemDef = JsonFile.Load<ItemDef>(filePath);

            if (string.IsNullOrWhiteSpace(itemDef.Id))
            {
                throw new InvalidOperationException($"Item definition in '{filePath}' is missing Id.");
            }

            if (!items.TryAdd(itemDef.Id, itemDef))
            {
                throw new InvalidOperationException(
                    $"Duplicate item id '{itemDef.Id}' found in '{filePath}'.");
            }
        }

        return items;
    }

    private static Dictionary<string, EnemyDef> LoadEnemyDefinitions(string enemiesPath)
    {
        if (!Directory.Exists(enemiesPath))
        {
            throw new DirectoryNotFoundException($"Enemies folder not found: {enemiesPath}");
        }

        var enemyFiles = Directory.GetFiles(enemiesPath, "*.json", SearchOption.TopDirectoryOnly);
        if (enemyFiles.Length == 0)
        {
            throw new InvalidOperationException($"No enemy definition files found in: {enemiesPath}");
        }

        var enemies = new Dictionary<string, EnemyDef>(StringComparer.Ordinal);

        foreach (var filePath in enemyFiles)
        {
            var enemyDef = JsonFile.Load<EnemyDef>(filePath);

            if (string.IsNullOrWhiteSpace(enemyDef.Id))
            {
                throw new InvalidOperationException($"Enemy definition in '{filePath}' is missing Id.");
            }

            if (!enemies.TryAdd(enemyDef.Id, enemyDef))
            {
                throw new InvalidOperationException(
                    $"Duplicate enemy id '{enemyDef.Id}' found in '{filePath}'.");
            }
        }

        return enemies;
    }

    private static Dictionary<string, EncounterDef> LoadEncounterDefinitions(string encountersPath)
    {
        if (!Directory.Exists(encountersPath))
        {
            throw new DirectoryNotFoundException($"Encounters folder not found: {encountersPath}");
        }

        var encounterFiles = Directory.GetFiles(encountersPath, "*.json", SearchOption.TopDirectoryOnly);
        if (encounterFiles.Length == 0)
        {
            throw new InvalidOperationException($"No encounter definition files found in: {encountersPath}");
        }

        var encounters = new Dictionary<string, EncounterDef>(StringComparer.Ordinal);

        foreach (var filePath in encounterFiles)
        {
            var encounterDef = JsonFile.Load<EncounterDef>(filePath);

            if (string.IsNullOrWhiteSpace(encounterDef.Id))
            {
                throw new InvalidOperationException($"Encounter definition in '{filePath}' is missing Id.");
            }

            if (!encounters.TryAdd(encounterDef.Id, encounterDef))
            {
                throw new InvalidOperationException(
                    $"Duplicate encounter id '{encounterDef.Id}' found in '{filePath}'.");
            }
        }

        return encounters;
    }

    private static Dictionary<string, EncounterTableDef> LoadEncounterTableDefinitions(string encounterTablesPath)
    {
        if (!Directory.Exists(encounterTablesPath))
        {
            throw new DirectoryNotFoundException($"Encounter tables folder not found: {encounterTablesPath}");
        }

        var encounterTableFiles = Directory.GetFiles(encounterTablesPath, "*.json", SearchOption.TopDirectoryOnly);
        if (encounterTableFiles.Length == 0)
        {
            throw new InvalidOperationException(
                $"No encounter table definition files found in: {encounterTablesPath}");
        }

        var encounterTables = new Dictionary<string, EncounterTableDef>(StringComparer.Ordinal);

        foreach (var filePath in encounterTableFiles)
        {
            var encounterTableDef = JsonFile.Load<EncounterTableDef>(filePath);

            if (string.IsNullOrWhiteSpace(encounterTableDef.Id))
            {
                throw new InvalidOperationException(
                    $"Encounter table definition in '{filePath}' is missing Id.");
            }

            if (!encounterTables.TryAdd(encounterTableDef.Id, encounterTableDef))
            {
                throw new InvalidOperationException(
                    $"Duplicate encounter table id '{encounterTableDef.Id}' found in '{filePath}'.");
            }
        }

        return encounterTables;
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
            if (string.IsNullOrWhiteSpace(mapDef.VisualAssetId))
            {
                throw new InvalidOperationException($"Map '{mapId}' must define a non-empty VisualAssetId.");
            }

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

            if (!mapDef.EncountersEnabled)
            {
                // Allowed: encounter fields may be default/ignored when encounters are disabled.
            }
            else
            {
                if (mapDef.EncounterRate <= 0)
                {
                    throw new InvalidOperationException(
                        $"Map '{mapId}' has encounters enabled but EncounterRate <= 0.");
                }

                if (string.IsNullOrWhiteSpace(mapDef.EncounterTableId))
                {
                    throw new InvalidOperationException(
                        $"Map '{mapId}' has encounters enabled but EncounterTableId is missing.");
                }
            }

            if (mapDef.EncounterRate > 100)
            {
                throw new InvalidOperationException(
                    $"Map '{mapId}' has EncounterRate > 100.");
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

            var seenSpawnIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var spawn in mapDef.Spawns)
            {
                if (string.IsNullOrWhiteSpace(spawn.Id))
                {
                    throw new InvalidOperationException(
                        $"Map '{mapId}' contains a spawn with missing Id.");
                }

                if (!seenSpawnIds.Add(spawn.Id))
                {
                    throw new InvalidOperationException(
                        $"Map '{mapId}' contains duplicate spawn id '{spawn.Id}'.");
                }

                if (spawn.X < 0 || spawn.X >= mapDef.Width ||
                    spawn.Y < 0 || spawn.Y >= mapDef.Height)
                {
                    throw new InvalidOperationException(
                        $"Map '{mapId}' spawn '{spawn.Id}' is out of bounds at ({spawn.X}, {spawn.Y}).");
                }
            }

            var seenObjectIds = new HashSet<string>(StringComparer.Ordinal);
            var occupiedObjectTiles = new HashSet<(int X, int Y)>();

            foreach (var mapObject in mapDef.Objects)
            {
                if (string.IsNullOrWhiteSpace(mapObject.Id))
                {
                    throw new InvalidOperationException(
                        $"Map '{mapId}' contains an object with missing Id.");
                }

                if (!seenObjectIds.Add(mapObject.Id))
                {
                    throw new InvalidOperationException(
                        $"Map '{mapId}' contains duplicate object id '{mapObject.Id}'.");
                }

                if (string.IsNullOrWhiteSpace(mapObject.Type))
                {
                    throw new InvalidOperationException(
                        $"Map '{mapId}' object '{mapObject.Id}' must have a non-empty Type.");
                }

                if (mapObject.X < 0 || mapObject.X >= mapDef.Width ||
                    mapObject.Y < 0 || mapObject.Y >= mapDef.Height)
                {
                    throw new InvalidOperationException(
                        $"Map '{mapId}' object '{mapObject.Id}' is out of bounds at ({mapObject.X}, {mapObject.Y}).");
                }

                if (!occupiedObjectTiles.Add((mapObject.X, mapObject.Y)))
                {
                    throw new InvalidOperationException(
                        $"Map '{mapId}' contains multiple objects on tile ({mapObject.X}, {mapObject.Y}).");
                }

                foreach (var blockedTile in mapDef.BlockedTiles)
                {
                    if (blockedTile.X == mapObject.X && blockedTile.Y == mapObject.Y)
                    {
                        throw new InvalidOperationException(
                            $"Map '{mapId}' object '{mapObject.Id}' is placed on blocked tile ({mapObject.X}, {mapObject.Y}).");
                    }
                }

                if (string.IsNullOrWhiteSpace(mapObject.InteractionId))
                {
                    throw new InvalidOperationException(
                        $"Map '{mapId}' object '{mapObject.Id}' must have a non-empty InteractionId.");
                }

                if (!string.IsNullOrWhiteSpace(mapObject.VisibleIfFlagSet) &&
                    !string.IsNullOrWhiteSpace(mapObject.VisibleIfFlagClear))
                {
                    throw new InvalidOperationException(
                        $"Map '{mapId}' object '{mapObject.Id}' cannot define both VisibleIfFlagSet and VisibleIfFlagClear.");
                }
            }

            var seenVariantIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (var variant in mapDef.StateVariants)
            {
                if (string.IsNullOrWhiteSpace(variant.Id))
                {
                    throw new InvalidOperationException(
                        $"Map '{mapId}' contains a state variant with missing Id.");
                }

                if (!seenVariantIds.Add(variant.Id))
                {
                    throw new InvalidOperationException(
                        $"Map '{mapId}' contains duplicate state variant id '{variant.Id}'.");
                }

                if (string.IsNullOrWhiteSpace(variant.FlagId))
                {
                    throw new InvalidOperationException(
                        $"Map '{mapId}' state variant '{variant.Id}' must have a non-empty FlagId.");
                }

                var seenVariantObjectIds = new HashSet<string>(StringComparer.Ordinal);

                foreach (var objectId in variant.EnableObjectIds)
                {
                    if (string.IsNullOrWhiteSpace(objectId))
                    {
                        throw new InvalidOperationException(
                            $"Map '{mapId}' state variant '{variant.Id}' contains an empty EnableObjectId.");
                    }

                    if (!seenObjectIds.Contains(objectId))
                    {
                        throw new InvalidOperationException(
                            $"Map '{mapId}' state variant '{variant.Id}' enables unknown object '{objectId}'.");
                    }

                    if (!seenVariantObjectIds.Add(objectId))
                    {
                        throw new InvalidOperationException(
                            $"Map '{mapId}' state variant '{variant.Id}' references object '{objectId}' more than once across enable/disable lists.");
                    }
                }

                foreach (var objectId in variant.DisableObjectIds)
                {
                    if (string.IsNullOrWhiteSpace(objectId))
                    {
                        throw new InvalidOperationException(
                            $"Map '{mapId}' state variant '{variant.Id}' contains an empty DisableObjectId.");
                    }

                    if (!seenObjectIds.Contains(objectId))
                    {
                        throw new InvalidOperationException(
                            $"Map '{mapId}' state variant '{variant.Id}' disables unknown object '{objectId}'.");
                    }

                    if (!seenVariantObjectIds.Add(objectId))
                    {
                        throw new InvalidOperationException(
                            $"Map '{mapId}' state variant '{variant.Id}' references object '{objectId}' more than once across enable/disable lists.");
                    }
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

    private static void ValidateDialogues(IReadOnlyDictionary<string, DialogueDef> dialogues)
    {
        foreach (var (dialogueId, dialogueDef) in dialogues)
        {
            if (dialogueDef.Variants.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Dialogue '{dialogueId}' must contain at least one variant.");
            }

            for (var variantIndex = 0; variantIndex < dialogueDef.Variants.Count; variantIndex++)
            {
                var variant = dialogueDef.Variants[variantIndex];

                if (variant.Lines.Count == 0)
                {
                    throw new InvalidOperationException(
                        $"Dialogue '{dialogueId}' variant {variantIndex} must contain at least one line.");
                }

                for (var lineIndex = 0; lineIndex < variant.Lines.Count; lineIndex++)
                {
                    if (string.IsNullOrWhiteSpace(variant.Lines[lineIndex]))
                    {
                        throw new InvalidOperationException(
                            $"Dialogue '{dialogueId}' variant {variantIndex} contains an empty line at index {lineIndex}.");
                    }
                }

                foreach (var result in variant.Results)
                {
                    ValidateInteractionResult(dialogueId, variantIndex, result);
                }
            }
        }
    }

    private static void ValidateInteractionResult(
        string dialogueId,
        int variantIndex,
        InteractionResultDef result)
    {
        if (string.IsNullOrWhiteSpace(result.Type))
        {
            throw new InvalidOperationException(
                $"Dialogue '{dialogueId}' variant {variantIndex} contains a result with missing Type.");
        }

        if (string.Equals(result.Type, "SetFlag", StringComparison.Ordinal))
        {
            if (string.IsNullOrWhiteSpace(result.FlagId))
            {
                throw new InvalidOperationException(
                    $"Dialogue '{dialogueId}' variant {variantIndex} has SetFlag result with missing FlagId.");
            }

            return;
        }

        if (string.Equals(result.Type, "GiveItem", StringComparison.Ordinal))
        {
            if (string.IsNullOrWhiteSpace(result.ItemId))
            {
                throw new InvalidOperationException(
                    $"Dialogue '{dialogueId}' variant {variantIndex} has GiveItem result with missing ItemId.");
            }

            if (result.Amount <= 0)
            {
                throw new InvalidOperationException(
                    $"Dialogue '{dialogueId}' variant {variantIndex} has GiveItem result with Amount <= 0.");
            }

            return;
        }

        throw new InvalidOperationException(
            $"Dialogue '{dialogueId}' variant {variantIndex} has unsupported result Type '{result.Type}'.");
    }

    private static void ValidateInteractions(IReadOnlyDictionary<string, InteractionDef> interactions)
    {
        foreach (var (interactionId, interactionDef) in interactions)
        {
            if (string.IsNullOrWhiteSpace(interactionDef.Type))
            {
                throw new InvalidOperationException(
                    $"Interaction '{interactionId}' must have a non-empty Type.");
            }

            if (string.Equals(interactionDef.Type, "DialogueNpc", StringComparison.Ordinal))
            {
                if (string.IsNullOrWhiteSpace(interactionDef.DialogueId))
                {
                    throw new InvalidOperationException(
                        $"Interaction '{interactionId}' must have a non-empty DialogueId.");
                }

                continue;
            }

            if (string.Equals(interactionDef.Type, "Chest", StringComparison.Ordinal))
            {
                if (string.IsNullOrWhiteSpace(interactionDef.DialogueId))
                {
                    throw new InvalidOperationException(
                        $"Chest interaction '{interactionId}' must have a non-empty DialogueId.");
                }

                if (string.IsNullOrWhiteSpace(interactionDef.ItemId))
                {
                    throw new InvalidOperationException(
                        $"Chest interaction '{interactionId}' must have a non-empty ItemId.");
                }

                if (interactionDef.Amount <= 0)
                {
                    throw new InvalidOperationException(
                        $"Chest interaction '{interactionId}' must have Amount > 0.");
                }

                if (string.IsNullOrWhiteSpace(interactionDef.OpenFlagId))
                {
                    throw new InvalidOperationException(
                        $"Chest interaction '{interactionId}' must have a non-empty OpenFlagId.");
                }

                continue;
            }

            if (string.Equals(interactionDef.Type, "LockedDoor", StringComparison.Ordinal))
            {
                if (string.IsNullOrWhiteSpace(interactionDef.RequiredFlagId))
                {
                    throw new InvalidOperationException(
                        $"Interaction '{interactionId}' of type 'LockedDoor' must have a non-empty RequiredFlagId.");
                }

                if (string.IsNullOrWhiteSpace(interactionDef.OpenFlagId))
                {
                    throw new InvalidOperationException(
                        $"Interaction '{interactionId}' of type 'LockedDoor' must have a non-empty OpenFlagId.");
                }

                if (string.IsNullOrWhiteSpace(interactionDef.OpenedDialogueId))
                {
                    throw new InvalidOperationException(
                        $"Interaction '{interactionId}' of type 'LockedDoor' must have a non-empty OpenedDialogueId.");
                }

                continue;
            }

            if (string.Equals(interactionDef.Type, "FlagGate", StringComparison.Ordinal))
            {
                if (string.IsNullOrWhiteSpace(interactionDef.RequiredFlagId))
                {
                    throw new InvalidOperationException(
                        $"Interaction '{interactionId}' of type 'FlagGate' must have a non-empty RequiredFlagId.");
                }

                continue;
            }

            if (string.Equals(interactionDef.Type, "MapExit", StringComparison.Ordinal))
            {
                if (string.IsNullOrWhiteSpace(interactionDef.DestinationMapId))
                {
                    throw new InvalidOperationException(
                        $"MapExit interaction '{interactionId}' must have a non-empty DestinationMapId.");
                }

                if (string.IsNullOrWhiteSpace(interactionDef.DestinationSpawnId))
                {
                    throw new InvalidOperationException(
                        $"MapExit interaction '{interactionId}' must have a non-empty DestinationSpawnId.");
                }

                continue;
            }

            throw new InvalidOperationException(
                $"Interaction '{interactionId}' has unsupported Type '{interactionDef.Type}'.");
        }
    }

    private static void ValidateItems(IReadOnlyDictionary<string, ItemDef> items)
    {
        foreach (var (itemId, itemDef) in items)
        {
            if (string.IsNullOrWhiteSpace(itemDef.Id))
            {
                throw new InvalidOperationException(
                    $"Item '{itemId}' must have a non-empty Id.");
            }

            if (string.IsNullOrWhiteSpace(itemDef.Name))
            {
                throw new InvalidOperationException(
                    $"Item '{itemId}' must have a non-empty Name.");
            }
        }
    }

    private static void ValidateEnemies(IReadOnlyDictionary<string, EnemyDef> enemies)
    {
        foreach (var (enemyId, enemyDef) in enemies)
        {
            if (string.IsNullOrWhiteSpace(enemyDef.Name))
            {
                throw new InvalidOperationException(
                    $"Enemy '{enemyId}' must have a non-empty Name.");
            }
        }
    }

    private static void ValidateEncounters(IReadOnlyDictionary<string, EncounterDef> encounters)
    {
        foreach (var (encounterId, encounterDef) in encounters)
        {
            if (encounterDef.EnemyIds.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Encounter '{encounterId}' must contain at least one enemy.");
            }

            if (encounterDef.EnemyIds.Count > 6)
            {
                throw new InvalidOperationException(
                    $"Encounter '{encounterId}' cannot contain more than 6 enemies.");
            }

            for (var i = 0; i < encounterDef.EnemyIds.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(encounterDef.EnemyIds[i]))
                {
                    throw new InvalidOperationException(
                        $"Encounter '{encounterId}' contains an empty enemy id at index {i}.");
                }
            }
        }
    }

    private static void ValidateEncounterTables(IReadOnlyDictionary<string, EncounterTableDef> encounterTables)
    {
        foreach (var (tableId, tableDef) in encounterTables)
        {
            if (tableDef.Entries.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Encounter table '{tableId}' must contain at least one entry.");
            }

            for (var i = 0; i < tableDef.Entries.Count; i++)
            {
                var entry = tableDef.Entries[i];

                if (string.IsNullOrWhiteSpace(entry.EncounterId))
                {
                    throw new InvalidOperationException(
                        $"Encounter table '{tableId}' contains an entry with empty EncounterId at index {i}.");
                }

                if (entry.Weight <= 0)
                {
                    throw new InvalidOperationException(
                        $"Encounter table '{tableId}' entry at index {i} must have Weight > 0.");
                }
            }
        }
    }

    private static void ValidateCrossReferences(
        GameConfig gameConfig,
        IReadOnlyDictionary<string, MapDef> maps,
        IReadOnlyDictionary<string, CharacterDef> characters,
        IReadOnlyDictionary<string, DialogueDef> dialogues,
        IReadOnlyDictionary<string, InteractionDef> interactions,
        IReadOnlyDictionary<string, ItemDef> items,
        IReadOnlyDictionary<string, EnemyDef> enemies,
        IReadOnlyDictionary<string, EncounterDef> encounters,
        IReadOnlyDictionary<string, EncounterTableDef> encounterTables)
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

        foreach (var mapObject in startingMap.Objects)
        {
            if (mapObject.X == gameConfig.StartingPlayerTileX &&
                mapObject.Y == gameConfig.StartingPlayerTileY &&
                mapObject.BlocksMovement)
            {
                throw new InvalidOperationException(
                    $"Starting player tile ({gameConfig.StartingPlayerTileX}, {gameConfig.StartingPlayerTileY}) " +
                    $"is occupied by blocking object '{mapObject.Id}' on map '{startingMap.Id}'.");
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

        foreach (var (mapId, mapDef) in maps)
        {
            foreach (var mapObject in mapDef.Objects)
            {
                if (!interactions.ContainsKey(mapObject.InteractionId))
                {
                    throw new InvalidOperationException(
                        $"Map '{mapId}' object '{mapObject.Id}' references missing interaction '{mapObject.InteractionId}'.");
                }
            }
        }

        foreach (var (interactionId, interactionDef) in interactions)
        {
            if (string.Equals(interactionDef.Type, "DialogueNpc", StringComparison.Ordinal))
            {
                if (!dialogues.ContainsKey(interactionDef.DialogueId))
                {
                    throw new InvalidOperationException(
                        $"Interaction '{interactionId}' references missing dialogue '{interactionDef.DialogueId}'.");
                }

                continue;
            }

            if (string.Equals(interactionDef.Type, "Chest", StringComparison.Ordinal))
            {
                if (!dialogues.ContainsKey(interactionDef.DialogueId))
                {
                    throw new InvalidOperationException(
                        $"Chest interaction '{interactionId}' references missing dialogue '{interactionDef.DialogueId}'.");
                }

                if (!string.IsNullOrWhiteSpace(interactionDef.OpenedDialogueId) &&
                    !dialogues.ContainsKey(interactionDef.OpenedDialogueId))
                {
                    throw new InvalidOperationException(
                        $"Chest interaction '{interactionId}' references missing opened dialogue '{interactionDef.OpenedDialogueId}'.");
                }

                if (!items.ContainsKey(interactionDef.ItemId))
                {
                    throw new InvalidOperationException(
                        $"Chest interaction '{interactionId}' references missing item '{interactionDef.ItemId}'.");
                }

                continue;
            }

            if (string.Equals(interactionDef.Type, "LockedDoor", StringComparison.Ordinal))
            {
                if (!string.IsNullOrWhiteSpace(interactionDef.DialogueId) &&
                    !dialogues.ContainsKey(interactionDef.DialogueId))
                {
                    throw new InvalidOperationException(
                        $"Interaction '{interactionId}' references missing dialogue '{interactionDef.DialogueId}'.");
                }

                if (!dialogues.ContainsKey(interactionDef.OpenedDialogueId))
                {
                    throw new InvalidOperationException(
                        $"Interaction '{interactionId}' references missing opened dialogue '{interactionDef.OpenedDialogueId}'.");
                }

                continue;
            }

            if (string.Equals(interactionDef.Type, "FlagGate", StringComparison.Ordinal))
            {
                if (!string.IsNullOrWhiteSpace(interactionDef.DialogueId) &&
                    !dialogues.ContainsKey(interactionDef.DialogueId))
                {
                    throw new InvalidOperationException(
                        $"Interaction '{interactionId}' references missing dialogue '{interactionDef.DialogueId}'.");
                }

                if (!string.IsNullOrWhiteSpace(interactionDef.OpenedDialogueId) &&
                    !dialogues.ContainsKey(interactionDef.OpenedDialogueId))
                {
                    throw new InvalidOperationException(
                        $"Interaction '{interactionId}' references missing opened dialogue '{interactionDef.OpenedDialogueId}'.");
                }

                continue;
            }

            if (string.Equals(interactionDef.Type, "MapExit", StringComparison.Ordinal))
            {
                if (!maps.TryGetValue(interactionDef.DestinationMapId, out var destinationMap))
                {
                    throw new InvalidOperationException(
                        $"MapExit interaction '{interactionId}' references missing destination map '{interactionDef.DestinationMapId}'.");
                }

                var foundSpawn = false;
                foreach (var spawn in destinationMap.Spawns)
                {
                    if (string.Equals(spawn.Id, interactionDef.DestinationSpawnId, StringComparison.Ordinal))
                    {
                        foundSpawn = true;
                        break;
                    }
                }

                if (!foundSpawn)
                {
                    throw new InvalidOperationException(
                        $"MapExit interaction '{interactionId}' references missing destination spawn '{interactionDef.DestinationSpawnId}' on map '{destinationMap.Id}'.");
                }

                continue;
            }

            throw new InvalidOperationException(
                $"Interaction '{interactionId}' has unsupported Type '{interactionDef.Type}'.");
        }
    
        foreach (var (mapId, mapDef) in maps)
        {
            if (!mapDef.EncountersEnabled)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(mapDef.EncounterTableId))
            {
                throw new InvalidOperationException(
                    $"Map '{mapId}' has encounters enabled but no EncounterTableId.");
            }

            if (!encounterTables.ContainsKey(mapDef.EncounterTableId))
            {
                throw new InvalidOperationException(
                    $"Map '{mapId}' references missing encounter table '{mapDef.EncounterTableId}'.");
            }
        }

        foreach (var (tableId, tableDef) in encounterTables)
        {
            foreach (var entry in tableDef.Entries)
            {
                if (!encounters.ContainsKey(entry.EncounterId))
                {
                    throw new InvalidOperationException(
                        $"Encounter table '{tableId}' references missing encounter '{entry.EncounterId}'.");
                }
            }
        }

        foreach (var (encounterId, encounterDef) in encounters)
        {
            foreach (var enemyId in encounterDef.EnemyIds)
            {
                if (!enemies.ContainsKey(enemyId))
                {
                    throw new InvalidOperationException(
                        $"Encounter '{encounterId}' references missing enemy '{enemyId}'.");
                }
            }
        }
    }
}