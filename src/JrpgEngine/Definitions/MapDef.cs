// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using JustTooFast.JrpgEngine.State;

namespace JustTooFast.JrpgEngine.Definitions;

public sealed class MapDef
{
    public string Id { get; set; } = string.Empty;

    public int Width { get; set; }

    public int Height { get; set; }

    public int TileSize { get; set; } = 32;

    public List<MapBlockedTileDef> BlockedTiles { get; set; } = new();

    public List<MapSpawnDef> Spawns { get; set; } = new();

    public List<MapObjectDef> Objects { get; set; } = new();
}

public sealed class MapBlockedTileDef
{
    public int X { get; set; }

    public int Y { get; set; }
}

public sealed class MapSpawnDef
{
    public string Id { get; set; } = string.Empty;

    public int X { get; set; }

    public int Y { get; set; }

    public FacingDirection Facing { get; set; }
}

public sealed class MapObjectDef
{
    public string Id { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public int X { get; set; }

    public int Y { get; set; }

    public bool BlocksMovement { get; set; } = true;

    public string InteractionId { get; set; } = string.Empty;
}