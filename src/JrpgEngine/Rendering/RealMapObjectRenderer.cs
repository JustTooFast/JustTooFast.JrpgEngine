// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.Definitions;
using JustTooFast.JrpgEngine.Maps;
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

        var tileSize = mapRuntime.TileSize;

        foreach (var mapObject in mapRuntime.Definition.Objects)
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

            var destinationRect = new Rectangle(
                mapObject.X * tileSize,
                mapObject.Y * tileSize,
                tileSize,
                tileSize);

            spriteBatch.Draw(texture, destinationRect, sourceRect, Color.White);
        }
    }
}