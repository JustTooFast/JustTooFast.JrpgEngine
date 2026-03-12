// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JustTooFast.JrpgEngine.Rendering;

public sealed class MapOverheadRenderer
{
    private readonly IVisualTextureStore _visualTextureStore;

    public MapOverheadRenderer(IVisualTextureStore visualTextureStore)
    {
        _visualTextureStore = visualTextureStore ?? throw new ArgumentNullException(nameof(visualTextureStore));
    }

    public void Draw(MapSceneRenderContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var overheadVisualAssetId = context.RuntimeMap.OverheadVisualAssetId;
        if (string.IsNullOrWhiteSpace(overheadVisualAssetId))
        {
            return;
        }

        var texture = _visualTextureStore.GetRequired(overheadVisualAssetId);

        context.SpriteBatch.Draw(texture, Vector2.Zero, Color.White);
    }
}