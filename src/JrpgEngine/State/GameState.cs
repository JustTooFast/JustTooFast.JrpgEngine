// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgEngine.State;

public sealed class GameState
{
    public GameState(
        string currentMapId,
        int playerTileX,
        int playerTileY,
        FacingDirection facing,
        PartyState party)
    {
        if (string.IsNullOrWhiteSpace(currentMapId))
        {
            throw new ArgumentException("Current map id cannot be null or empty.", nameof(currentMapId));
        }

        CurrentMapId = currentMapId;
        PlayerTileX = playerTileX;
        PlayerTileY = playerTileY;
        Facing = facing;
        Party = party ?? throw new ArgumentNullException(nameof(party));
    }

    public string CurrentMapId { get; set; }

    public int PlayerTileX { get; set; }

    public int PlayerTileY { get; set; }

    public FacingDirection Facing { get; set; }

    public PartyState Party { get; }

    public StoryFlagState StoryFlags { get; } = new();

    public InventoryState Inventory { get; } = new();

    public bool IsPaused { get; set; }

    public PendingMapTransitionState? PendingMapTransition { get; set; }
}

public sealed class PendingMapTransitionState
{
    public string DestinationMapId { get; set; } = string.Empty;

    public string DestinationSpawnId { get; set; } = string.Empty;
}