// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.Definitions;
using Microsoft.Xna.Framework;

namespace JustTooFast.JrpgEngine.Rendering;

public sealed class RealPlayerRenderer : IPlayerRenderer
{
    private readonly DefinitionDatabase _definitions;
    private readonly IVisualTextureStore _visualTextureStore;

    public RealPlayerRenderer(
        DefinitionDatabase definitions,
        IVisualTextureStore visualTextureStore)
    {
        _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
        _visualTextureStore = visualTextureStore ?? throw new ArgumentNullException(nameof(visualTextureStore));
    }

    public void Draw(PlayerRenderContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (!_definitions.Characters.TryGetValue(context.LeaderCharacterId, out var characterDef))
        {
            throw new InvalidOperationException(
                $"Leader character '{context.LeaderCharacterId}' was not found in definitions.");
        }

        if (!_definitions.Visuals.TryGetValue(characterDef.VisualId, out var visualDef))
        {
            throw new InvalidOperationException(
                $"Character '{characterDef.Id}' references missing visual '{characterDef.VisualId}'.");
        }

        var texture = _visualTextureStore.GetRequired(visualDef.VisualAssetId);

        var row = VisualSourceRectHelper.GetFacingRow(visualDef, context.FacingDirection);
        var sourceRect = VisualSourceRectHelper.GetSourceRect(visualDef, row, frameIndex: 0);

        var destinationRect = new Rectangle(
            (int)MathF.Round(context.ScreenPosition.X),
            (int)MathF.Round(context.ScreenPosition.Y),
            context.TileSize,
            context.TileSize);

        context.SpriteBatch.Draw(texture, destinationRect, sourceRect, Color.White);
    }
}