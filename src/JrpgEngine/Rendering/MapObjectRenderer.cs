// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgEngine.Rendering;

public sealed class MapObjectRenderer : IMapObjectRenderer
{
    private readonly PresentationMode _presentationMode;
    private readonly IMapObjectRenderer _debugRenderer;
    private readonly IMapObjectRenderer _realRenderer;

    public MapObjectRenderer(
        PresentationMode presentationMode,
        IMapObjectRenderer debugRenderer,
        IMapObjectRenderer realRenderer)
    {
        _presentationMode = presentationMode;
        _debugRenderer = debugRenderer ?? throw new ArgumentNullException(nameof(debugRenderer));
        _realRenderer = realRenderer ?? throw new ArgumentNullException(nameof(realRenderer));
    }

    public void Draw(MapSceneRenderContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var renderer = _presentationMode == PresentationMode.Real
            ? _realRenderer
            : _debugRenderer;

        renderer.Draw(context);
    }
}