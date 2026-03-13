// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.Definitions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JustTooFast.JrpgEngine.Rendering;

public sealed class RealMapObjectRenderer : IMapObjectRenderer
{
    private readonly IVisualTextureStore _visualTextureStore;
    private readonly DefinitionDatabase _definitions;

    public RealMapObjectRenderer(
        IVisualTextureStore visualTextureStore,
        DefinitionDatabase definitions)
    {
        _visualTextureStore = visualTextureStore ?? throw new ArgumentNullException(nameof(visualTextureStore));
        _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
    }

    public void Draw(MapSceneRenderContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var tileSize = context.RuntimeMap.TileSize;

        foreach (var mapObject in context.RuntimeMap.Definition.Objects)
        {
            if (!_definitions.MapObjects.TryGetValue(mapObject.MapObjectDefId, out var mapObjectDef))
            {
                throw new InvalidOperationException(
                    $"Map object placement '{mapObject.Id}' references unknown map object def '{mapObject.MapObjectDefId}'.");
            }

            if (string.IsNullOrWhiteSpace(mapObjectDef.VisualId))
            {
                continue;
            }

            if (!_definitions.Visuals.TryGetValue(mapObjectDef.VisualId, out var visualDef))
            {
                throw new InvalidOperationException(
                    $"Map object def '{mapObjectDef.Id}' references unknown visual '{mapObjectDef.VisualId}'.");
            }

            var texture = _visualTextureStore.GetRequired(visualDef.VisualAssetId);
            var sourceRect = VisualSourceRectHelper.GetSourceRect(visualDef, row: 0, frameIndex: 0);

            var worldRect = new Rectangle(
                mapObject.X * tileSize,
                mapObject.Y * tileSize,
                tileSize,
                tileSize);

            var destinationRect = context.WorldToScreen(worldRect);

            context.SpriteBatch.Draw(texture, destinationRect, sourceRect, Color.White);
        }
    }
}