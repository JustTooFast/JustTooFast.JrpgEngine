// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.State;
using Microsoft.Xna.Framework;

namespace JustTooFast.JrpgEngine.Rendering;

public sealed class MapCamera
{
    private readonly int _viewportWidth;
    private readonly int _viewportHeight;
    private readonly int _leadTiles;
    private readonly float _followLerpPerSecond;

    private FacingDirection _leadDirection;

    public MapCamera(
        int viewportWidth,
        int viewportHeight,
        int leadTiles,
        float followLerpPerSecond = 2f)
    {
        if (viewportWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(viewportWidth), "Viewport width must be > 0.");
        }

        if (viewportHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(viewportHeight), "Viewport height must be > 0.");
        }

        if (leadTiles < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(leadTiles), "Lead tiles must be >= 0.");
        }

        if (followLerpPerSecond <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(followLerpPerSecond), "Follow lerp must be > 0.");
        }

        _viewportWidth = viewportWidth;
        _viewportHeight = viewportHeight;
        _leadTiles = leadTiles;
        _followLerpPerSecond = followLerpPerSecond;
        _leadDirection = FacingDirection.Down;
    }

    public Vector2 Position { get; private set; }

    public void SnapTo(
        Vector2 playerWorldPosition,
        FacingDirection facingDirection,
        bool isMoving,
        int tileSize,
        int mapPixelWidth,
        int mapPixelHeight)
    {
        if (tileSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tileSize), "Tile size must be > 0.");
        }

        if (mapPixelWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(mapPixelWidth), "Map pixel width must be > 0.");
        }

        if (mapPixelHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(mapPixelHeight), "Map pixel height must be > 0.");
        }

        if (isMoving)
        {
            _leadDirection = facingDirection;
        }

        Position = CalculateTargetPosition(
            playerWorldPosition,
            isMoving,
            tileSize,
            mapPixelWidth,
            mapPixelHeight);
    }

    public void Update(
        GameTime gameTime,
        Vector2 playerWorldPosition,
        FacingDirection facingDirection,
        bool isMoving,
        int tileSize,
        int mapPixelWidth,
        int mapPixelHeight)
    {
        if (gameTime is null)
        {
            throw new ArgumentNullException(nameof(gameTime));
        }

        if (tileSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tileSize), "Tile size must be > 0.");
        }

        if (mapPixelWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(mapPixelWidth), "Map pixel width must be > 0.");
        }

        if (mapPixelHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(mapPixelHeight), "Map pixel height must be > 0.");
        }

        if (isMoving)
        {
            _leadDirection = facingDirection;
        }

        var target = CalculateTargetPosition(
            playerWorldPosition,
            isMoving,
            tileSize,
            mapPixelWidth,
            mapPixelHeight);

        var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var t = MathHelper.Clamp(deltaSeconds * _followLerpPerSecond, 0f, 1f);

        Position = Vector2.Lerp(Position, target, t);
    }

    // Lead applies only during active movement.
    // When idle, target position is centered on the player with no lead offset.
    private Vector2 CalculateTargetPosition(
        Vector2 playerWorldPosition,
        bool isMoving,
        int tileSize,
        int mapPixelWidth,
        int mapPixelHeight)
    {
        var leadOffset = Vector2.Zero;

        if (isMoving)
        {
            var leadPixels = _leadTiles * tileSize;
            leadOffset = GetLeadOffset(_leadDirection, leadPixels);
        }

        var targetX = playerWorldPosition.X + leadOffset.X - (_viewportWidth / 2f) + (tileSize / 2f);
        var targetY = playerWorldPosition.Y + leadOffset.Y - (_viewportHeight / 2f) + (tileSize / 2f);

        targetX = ResolveAxis(targetX, mapPixelWidth, _viewportWidth);
        targetY = ResolveAxis(targetY, mapPixelHeight, _viewportHeight);

        return new Vector2(targetX, targetY);
    }

    private static float ResolveAxis(float target, int mapPixels, int viewportPixels)
    {
        if (mapPixels <= viewportPixels)
        {
            return -((viewportPixels - mapPixels) / 2f);
        }

        var max = mapPixels - viewportPixels;
        return MathHelper.Clamp(target, 0f, max);
    }

    private static Vector2 GetLeadOffset(FacingDirection direction, int leadPixels)
    {
        return direction switch
        {
            FacingDirection.Up => new Vector2(0f, -leadPixels),
            FacingDirection.Down => new Vector2(0f, leadPixels),
            FacingDirection.Left => new Vector2(-leadPixels, 0f),
            FacingDirection.Right => new Vector2(leadPixels, 0f),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Unsupported direction.")
        };
    }
}