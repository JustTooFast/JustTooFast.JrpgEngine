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

    public string? VisualAssetId { get; set; }

    public string? OverheadVisualAssetId { get; set; }

    public bool EncountersEnabled { get; set; }

    public int EncounterRate { get; set; }

    public string? EncounterTableId { get; set; }

    public List<MapBlockedTileDef> BlockedTiles { get; set; } = new();

    public List<MapSpawnDef> Spawns { get; set; } = new();

    public List<MapObjectDef> Objects { get; set; } = new();

    public List<MapStateVariantDef> StateVariants { get; set; } = new();
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

    public string? VisibleIfFlagSet { get; set; }

    public string? VisibleIfFlagClear { get; set; }
}

public sealed class MapStateVariantDef
{
    public string Id { get; set; } = string.Empty;

    public string FlagId { get; set; } = string.Empty;

    public bool ActiveWhenSet { get; set; } = true;

    public string? VisualAssetOverrideId { get; set; }

    public string? OverheadVisualAssetOverrideId { get; set; }

    public List<string> EnableObjectIds { get; set; } = new();

    public List<string> DisableObjectIds { get; set; } = new();
}