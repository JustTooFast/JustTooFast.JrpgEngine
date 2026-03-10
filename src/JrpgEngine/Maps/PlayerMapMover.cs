// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.Definitions;
using JustTooFast.JrpgEngine.State;
using Microsoft.Xna.Framework;

namespace JustTooFast.JrpgEngine.Maps;

public sealed class PlayerMapMover
{
    public bool IsMoving { get; private set; }

    public TileCoord MoveFromTile { get; private set; }

    public TileCoord MoveToTile { get; private set; }

    public float MoveProgress { get; private set; }

    public float MoveDurationSeconds { get; set; } = 0.18f;

    public void InitializeFromGameState(GameState gameState)
    {
        if (gameState is null)
        {
            throw new ArgumentNullException(nameof(gameState));
        }

        var currentTile = new TileCoord(gameState.PlayerTileX, gameState.PlayerTileY);

        IsMoving = false;
        MoveFromTile = currentTile;
        MoveToTile = currentTile;
        MoveProgress = 0f;
    }

    public void Update(GameTime gameTime, GameState gameState, MapDef mapDef)
    {
        if (gameTime is null)
        {
            throw new ArgumentNullException(nameof(gameTime));
        }

        if (gameState is null)
        {
            throw new ArgumentNullException(nameof(gameState));
        }

        if (mapDef is null)
        {
            throw new ArgumentNullException(nameof(mapDef));
        }

        if (!IsMoving)
        {
            return;
        }

        var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (MoveDurationSeconds <= 0f)
        {
            MoveDurationSeconds = 0.18f;
        }

        MoveProgress += deltaSeconds / MoveDurationSeconds;

        if (MoveProgress >= 1f)
        {
            CommitCompletedMove(gameState);
        }
    }

    public void TryBeginMove(FacingDirection direction, GameState gameState)
    {
        if (gameState is null)
        {
            throw new ArgumentNullException(nameof(gameState));
        }

        if (IsMoving)
        {
            return;
        }

        gameState.Facing = direction;

        var currentTile = new TileCoord(gameState.PlayerTileX, gameState.PlayerTileY);
        var destinationTile = currentTile + GetDirectionOffset(direction);

        IsMoving = true;
        MoveFromTile = currentTile;
        MoveToTile = destinationTile;
        MoveProgress = 0f;
    }

    public Vector2 GetVisualWorldPosition(GameState gameState, int tileSize)
    {
        if (gameState is null)
        {
            throw new ArgumentNullException(nameof(gameState));
        }

        if (tileSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tileSize), "Tile size must be > 0.");
        }

        if (!IsMoving)
        {
            return TileToWorld(new TileCoord(gameState.PlayerTileX, gameState.PlayerTileY), tileSize);
        }

        var fromWorld = TileToWorld(MoveFromTile, tileSize);
        var toWorld = TileToWorld(MoveToTile, tileSize);

        return Vector2.Lerp(fromWorld, toWorld, Math.Clamp(MoveProgress, 0f, 1f));
    }

    private void CommitCompletedMove(GameState gameState)
    {
        gameState.PlayerTileX = MoveToTile.X;
        gameState.PlayerTileY = MoveToTile.Y;

        IsMoving = false;
        MoveFromTile = MoveToTile;
        MoveProgress = 0f;

        OnSuccessfulTileMoveCommitted(gameState);
    }

    private void OnSuccessfulTileMoveCommitted(GameState gameState)
    {
    }

    private static TileCoord GetDirectionOffset(FacingDirection direction)
    {
        return direction switch
        {
            FacingDirection.Up => new TileCoord(0, -1),
            FacingDirection.Down => new TileCoord(0, 1),
            FacingDirection.Left => new TileCoord(-1, 0),
            FacingDirection.Right => new TileCoord(1, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Unsupported direction.")
        };
    }

    private static Vector2 TileToWorld(TileCoord tile, int tileSize)
    {
        return new Vector2(tile.X * tileSize, tile.Y * tileSize);
    }
}