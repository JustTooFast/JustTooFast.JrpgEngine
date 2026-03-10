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
        MapRuntime runtimeMap)
    {
        SpriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
        GameTime = gameTime ?? throw new ArgumentNullException(nameof(gameTime));
        GameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
        RuntimeMap = runtimeMap ?? throw new ArgumentNullException(nameof(runtimeMap));
    }

    public SpriteBatch SpriteBatch { get; }

    public GameTime GameTime { get; }

    public GameState GameState { get; }

    public MapRuntime RuntimeMap { get; }
}