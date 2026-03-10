// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.Definitions;

namespace JustTooFast.JrpgEngine.Maps;

public sealed class MapRuntime
{
    public MapRuntime(
        MapDef definition,
        string? activeVariantId = null,
        string? visualStyleId = null)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        ActiveVariantId = activeVariantId;
        VisualStyleId = visualStyleId;
    }

    public MapDef Definition { get; }

    public string? ActiveVariantId { get; }

    public string? VisualStyleId { get; }

    public string MapId => Definition.Id;

    public int Width => Definition.Width;

    public int Height => Definition.Height;

    public int TileSize => Definition.TileSize;
}