// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgEngine.Rendering;

public sealed class MapBackgroundRenderer : IMapBackgroundRenderer
{
    private readonly IMapBackgroundRenderer _innerRenderer;

    public MapBackgroundRenderer(
        PresentationMode presentationMode,
        IMapBackgroundRenderer debugRenderer,
        IMapBackgroundRenderer realRenderer)
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

    public void Draw(MapSceneRenderContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        _innerRenderer.Draw(context);
    }
}