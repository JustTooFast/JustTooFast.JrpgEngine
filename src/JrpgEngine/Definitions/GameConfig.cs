// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace JustTooFast.JrpgEngine.Definitions;

public sealed class GameConfig
{
    public string StartingMapId { get; set; } = string.Empty;

    public int StartingPlayerTileX { get; set; }

    public int StartingPlayerTileY { get; set; }

    public string StartingFacing { get; set; } = "Down";

    public List<string> StartingPartyMemberIds { get; set; } = new();
}