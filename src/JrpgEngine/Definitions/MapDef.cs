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

    public List<MapBlockedTileDef> BlockedTiles { get; set; } = new();
}

public sealed class MapBlockedTileDef
{
    public int X { get; set; }

    public int Y { get; set; }
}