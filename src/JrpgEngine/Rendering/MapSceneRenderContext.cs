// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.Maps;
using JustTooFast.JrpgEngine.State;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JustTooFast.JrpgEngine.Rendering;

public sealed class MapSceneRenderContext
{
    public MapSceneRenderContext(
        SpriteBatch spriteBatch,
        GameTime gameTime,
        GameState gameState,
        MapRuntime runtimeMap,
        Vector2 cameraWorldPosition,
        int viewportWidth,
        int viewportHeight)
    {
        SpriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
        GameTime = gameTime ?? throw new ArgumentNullException(nameof(gameTime));
        GameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
        RuntimeMap = runtimeMap ?? throw new ArgumentNullException(nameof(runtimeMap));

        if (viewportWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(viewportWidth), "Viewport width must be > 0.");
        }

        if (viewportHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(viewportHeight), "Viewport height must be > 0.");
        }

        CameraWorldPosition = cameraWorldPosition;
        ViewportWidth = viewportWidth;
        ViewportHeight = viewportHeight;
    }

    public SpriteBatch SpriteBatch { get; }

    public GameTime GameTime { get; }

    public GameState GameState { get; }

    public MapRuntime RuntimeMap { get; }

    public Vector2 CameraWorldPosition { get; }

    public int ViewportWidth { get; }

    public int ViewportHeight { get; }

    public Vector2 WorldToScreen(Vector2 worldPosition)
    {
        return worldPosition - CameraWorldPosition;
    }

    public Rectangle WorldToScreen(Rectangle worldRectangle)
    {
        return new Rectangle(
            (int)MathF.Round(worldRectangle.X - CameraWorldPosition.X),
            (int)MathF.Round(worldRectangle.Y - CameraWorldPosition.Y),
            worldRectangle.Width,
            worldRectangle.Height);
    }
}