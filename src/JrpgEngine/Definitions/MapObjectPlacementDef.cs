// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

namespace JustTooFast.JrpgEngine.Definitions;

public sealed class MapObjectPlacementDef
{
    public string Id { get; set; } = string.Empty;

    public int X { get; set; }

    public int Y { get; set; }

    public string MapObjectDefId { get; set; } = string.Empty;

    public string InteractionId { get; set; } = string.Empty;

    public string? VisibleIfFlagSet { get; set; }

    public string? VisibleIfFlagClear { get; set; }
}