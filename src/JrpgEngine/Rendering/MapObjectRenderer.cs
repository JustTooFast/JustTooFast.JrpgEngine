// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.Maps;
using Microsoft.Xna.Framework.Graphics;

namespace JustTooFast.JrpgEngine.Rendering;

public sealed class MapObjectRenderer : IMapObjectRenderer
{
    private readonly IMapObjectRenderer _innerRenderer;

    public MapObjectRenderer(
        PresentationMode presentationMode,
        IMapObjectRenderer debugRenderer,
        IMapObjectRenderer realRenderer)
    {
        if (debugRenderer is null)
        {
            throw new ArgumentNullException(nameof(debugRenderer));
        }

        if (realRenderer is null)
        {
            throw new ArgumentNullException(nameof(realRenderer));
        }

        _innerRenderer = presentationMode switch
        {
            PresentationMode.Debug => debugRenderer,
            PresentationMode.Real => realRenderer,
            _ => throw new ArgumentOutOfRangeException(
                nameof(presentationMode),
                presentationMode,
                "Unsupported presentation mode.")
        };
    }

    public void Draw(SpriteBatch spriteBatch, MapRuntime mapRuntime)
    {
        if (spriteBatch is null)
        {
            throw new ArgumentNullException(nameof(spriteBatch));
        }

        if (mapRuntime is null)
        {
            throw new ArgumentNullException(nameof(mapRuntime));
        }

        _innerRenderer.Draw(spriteBatch, mapRuntime);
    }
}