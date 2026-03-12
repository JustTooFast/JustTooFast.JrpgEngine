// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.Definitions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JustTooFast.JrpgEngine.Rendering;

public sealed class RealPlayerRenderer : IPlayerRenderer
{
    private readonly IVisualTextureStore _visualTextureStore;
    private readonly DefinitionDatabase _definitions;
    private readonly CharacterSpriteSheetLayout _spriteSheetLayout;

    public RealPlayerRenderer(
        IVisualTextureStore visualTextureStore,
        DefinitionDatabase definitions,
        CharacterSpriteSheetLayout spriteSheetLayout)
    {
        _visualTextureStore = visualTextureStore ?? throw new ArgumentNullException(nameof(visualTextureStore));
        _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
        _spriteSheetLayout = spriteSheetLayout ?? throw new ArgumentNullException(nameof(spriteSheetLayout));
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

        if (string.IsNullOrWhiteSpace(characterDef.VisualAssetId))
        {
            throw new InvalidOperationException(
                $"Character '{characterDef.Id}' must define a non-empty VisualAssetId.");
        }

        var texture = _visualTextureStore.GetRequired(characterDef.VisualAssetId);
        var sourceRect = _spriteSheetLayout.GetSourceRect(context.FacingDirection, frameIndex: 0);

        var destinationRect = new Rectangle(
            (int)context.WorldPosition.X,
            (int)context.WorldPosition.Y,
            context.TileSize,
            context.TileSize);

        context.SpriteBatch.Draw(texture, destinationRect, sourceRect, Color.White);
    }
}