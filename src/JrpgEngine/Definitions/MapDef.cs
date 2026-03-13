// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

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

    public List<MapObjectPlacementDef> Objects { get; set; } = new();

    public List<MapStateVariantDef> StateVariants { get; set; } = new();
}