// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using JustTooFast.JrpgEngine.Definitions;

namespace JustTooFast.JrpgEngine.Maps;

public sealed class MapCollisionService
{
    private readonly Dictionary<string, HashSet<TileCoord>> _blockedTileCache =
        new(StringComparer.Ordinal);

    public bool IsInBounds(MapDef mapDef, TileCoord tile)
    {
        if (mapDef is null)
        {
            throw new ArgumentNullException(nameof(mapDef));
        }

        return tile.X >= 0 &&
               tile.X < mapDef.Width &&
               tile.Y >= 0 &&
               tile.Y < mapDef.Height;
    }

    public bool IsBlocked(MapDef mapDef, TileCoord tile)
    {
        if (mapDef is null)
        {
            throw new ArgumentNullException(nameof(mapDef));
        }

        if (!IsInBounds(mapDef, tile))
        {
            return true;
        }

        var blockedTiles = GetOrBuildBlockedTileSet(mapDef);
        return blockedTiles.Contains(tile);
    }

    public bool CanEnterTile(MapDef mapDef, TileCoord tile)
    {
        return !IsBlocked(mapDef, tile);
    }

    private HashSet<TileCoord> GetOrBuildBlockedTileSet(MapDef mapDef)
    {
        if (_blockedTileCache.TryGetValue(mapDef.Id, out var cached))
        {
            return cached;
        }

        var built = new HashSet<TileCoord>();
        foreach (var blockedTile in mapDef.BlockedTiles)
        {
            built.Add(new TileCoord(blockedTile.X, blockedTile.Y));
        }

        _blockedTileCache.Add(mapDef.Id, built);
        return built;
    }
}