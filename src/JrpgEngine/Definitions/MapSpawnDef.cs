// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using JustTooFast.JrpgEngine.State;

namespace JustTooFast.JrpgEngine.Definitions;

public sealed class MapSpawnDef
{
    public string Id { get; set; } = string.Empty;

    public int X { get; set; }

    public int Y { get; set; }

    public FacingDirection Facing { get; set; }
}