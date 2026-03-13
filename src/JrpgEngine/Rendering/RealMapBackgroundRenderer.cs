// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JustTooFast.JrpgEngine.Rendering;

public sealed class RealMapBackgroundRenderer : IMapBackgroundRenderer
{
    private readonly IVisualTextureStore _visualTextureStore;

    public RealMapBackgroundRenderer(IVisualTextureStore visualTextureStore)
    {
        _visualTextureStore = visualTextureStore ?? throw new ArgumentNullException(nameof(visualTextureStore));
    }

    public void Draw(MapSceneRenderContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var visualAssetId = context.RuntimeMap.VisualAssetId;
        if (string.IsNullOrWhiteSpace(visualAssetId))
        {
            return;
        }

        var texture = _visualTextureStore.GetRequired(visualAssetId);

        var destination = new Vector2(
            -context.CameraWorldPosition.X,
            -context.CameraWorldPosition.Y);

        context.SpriteBatch.Draw(texture, destination, Color.White);
    }
}