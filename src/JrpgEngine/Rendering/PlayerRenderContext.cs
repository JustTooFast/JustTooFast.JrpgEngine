// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.State;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JustTooFast.JrpgEngine.Rendering;

public sealed class PlayerRenderContext
{
    public PlayerRenderContext(
        SpriteBatch spriteBatch,
        Vector2 worldPosition,
        Vector2 screenPosition,
        int tileSize,
        FacingDirection facingDirection,
        string leaderCharacterId)
    {
        SpriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));

        if (tileSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tileSize), "Tile size must be > 0.");
        }

        if (string.IsNullOrWhiteSpace(leaderCharacterId))
        {
            throw new ArgumentException("Leader character id cannot be null or empty.", nameof(leaderCharacterId));
        }

        WorldPosition = worldPosition;
        ScreenPosition = screenPosition;
        TileSize = tileSize;
        FacingDirection = facingDirection;
        LeaderCharacterId = leaderCharacterId;
    }

    public SpriteBatch SpriteBatch { get; }

    public Vector2 WorldPosition { get; }

    public Vector2 ScreenPosition { get; }

    public int TileSize { get; }

    public FacingDirection FacingDirection { get; }

    public string LeaderCharacterId { get; }
}