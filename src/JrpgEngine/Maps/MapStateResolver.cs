// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using JustTooFast.JrpgEngine.Definitions;
using JustTooFast.JrpgEngine.State;

namespace JustTooFast.JrpgEngine.Maps;

public sealed class MapStateResolver
{
    public ResolvedMapState Resolve(MapDef sourceMap, StoryFlagState storyFlags)
    {
        if (sourceMap is null)
        {
            throw new ArgumentNullException(nameof(sourceMap));
        }

        if (storyFlags is null)
        {
            throw new ArgumentNullException(nameof(storyFlags));
        }

        var activeVariant = ResolveActiveVariant(sourceMap, storyFlags);
        var resolvedObjects = ResolveVisibleObjects(sourceMap, storyFlags, activeVariant);
        var effectiveMapDef = CloneMapWithResolvedObjects(sourceMap, resolvedObjects);

        var visualStyleId = string.IsNullOrWhiteSpace(activeVariant?.VisualStyleId)
            ? null
            : activeVariant.VisualStyleId;

        return new ResolvedMapState(
            effectiveMapDef,
            activeVariant?.Id,
            visualStyleId);
    }

    private static MapStateVariantDef? ResolveActiveVariant(MapDef mapDef, StoryFlagState storyFlags)
    {
        foreach (var variant in mapDef.StateVariants)
        {
            var isSet = storyFlags.IsSet(variant.FlagId);
            if (isSet == variant.ActiveWhenSet)
            {
                return variant;
            }
        }

        return null;
    }

    private static List<MapObjectDef> ResolveVisibleObjects(
        MapDef mapDef,
        StoryFlagState storyFlags,
        MapStateVariantDef? activeVariant)
    {
        var disabledObjectIds = new HashSet<string>(StringComparer.Ordinal);
        var enabledObjectIds = new HashSet<string>(StringComparer.Ordinal);

        if (activeVariant is not null)
        {
            foreach (var objectId in activeVariant.DisableObjectIds)
            {
                disabledObjectIds.Add(objectId);
            }

            foreach (var objectId in activeVariant.EnableObjectIds)
            {
                enabledObjectIds.Add(objectId);
            }
        }

        var resolvedObjects = new List<MapObjectDef>();

        foreach (var mapObject in mapDef.Objects)
        {
            // Visibility precedence:
            // 1. object visibility conditions
            // 2. active variant disable list
            // 3. active variant enable list
            var isVisible = IsVisibleByObjectConditions(mapObject, storyFlags);

            if (disabledObjectIds.Contains(mapObject.Id))
            {
                isVisible = false;
            }

            if (enabledObjectIds.Contains(mapObject.Id))
            {
                isVisible = true;
            }

            if (isVisible)
            {
                resolvedObjects.Add(CloneObject(mapObject));
            }
        }

        return resolvedObjects;
    }

    private static bool IsVisibleByObjectConditions(MapObjectDef mapObject, StoryFlagState storyFlags)
    {
        if (!string.IsNullOrWhiteSpace(mapObject.VisibleIfFlagSet) &&
            !string.IsNullOrWhiteSpace(mapObject.VisibleIfFlagClear))
        {
            throw new InvalidOperationException(
                $"Map object '{mapObject.Id}' cannot define both VisibleIfFlagSet and VisibleIfFlagClear.");
        }

        if (!string.IsNullOrWhiteSpace(mapObject.VisibleIfFlagSet))
        {
            return storyFlags.IsSet(mapObject.VisibleIfFlagSet);
        }

        if (!string.IsNullOrWhiteSpace(mapObject.VisibleIfFlagClear))
        {
            return !storyFlags.IsSet(mapObject.VisibleIfFlagClear);
        }

        return true;
    }

    private static MapDef CloneMapWithResolvedObjects(
        MapDef sourceMap,
        List<MapObjectDef> resolvedObjects)
    {
        return new MapDef
        {
            Id = sourceMap.Id,
            Width = sourceMap.Width,
            Height = sourceMap.Height,
            TileSize = sourceMap.TileSize,
            BlockedTiles = CloneBlockedTiles(sourceMap.BlockedTiles),
            Spawns = CloneSpawns(sourceMap.Spawns),
            Objects = resolvedObjects,
            StateVariants = new List<MapStateVariantDef>()
        };
    }

    private static List<MapBlockedTileDef> CloneBlockedTiles(List<MapBlockedTileDef> blockedTiles)
    {
        var clone = new List<MapBlockedTileDef>(blockedTiles.Count);

        foreach (var blockedTile in blockedTiles)
        {
            clone.Add(new MapBlockedTileDef
            {
                X = blockedTile.X,
                Y = blockedTile.Y
            });
        }

        return clone;
    }

    private static List<MapSpawnDef> CloneSpawns(List<MapSpawnDef> spawns)
    {
        var clone = new List<MapSpawnDef>(spawns.Count);

        foreach (var spawn in spawns)
        {
            clone.Add(new MapSpawnDef
            {
                Id = spawn.Id,
                X = spawn.X,
                Y = spawn.Y,
                Facing = spawn.Facing
            });
        }

        return clone;
    }

    private static MapObjectDef CloneObject(MapObjectDef mapObject)
    {
        return new MapObjectDef
        {
            Id = mapObject.Id,
            Type = mapObject.Type,
            X = mapObject.X,
            Y = mapObject.Y,
            BlocksMovement = mapObject.BlocksMovement,
            InteractionId = mapObject.InteractionId,
            VisibleIfFlagSet = mapObject.VisibleIfFlagSet,
            VisibleIfFlagClear = mapObject.VisibleIfFlagClear
        };
    }
}