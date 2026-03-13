// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JustTooFast.JrpgEngine.Rendering;

public sealed class DebugPlayerRenderer : IPlayerRenderer
{
    private readonly Texture2D _pixel;

    public DebugPlayerRenderer(Texture2D pixel)
    {
        _pixel = pixel ?? throw new ArgumentNullException(nameof(pixel));
    }

    public void Draw(PlayerRenderContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var spriteBatch = context.SpriteBatch;
        var screenPosition = context.ScreenPosition;
        var tileSize = context.TileSize;

        var inset = Math.Max(4, tileSize / 8);

        var bounds = new Rectangle(
            (int)MathF.Round(screenPosition.X) + inset,
            (int)MathF.Round(screenPosition.Y) + inset,
            tileSize - (inset * 2),
            tileSize - (inset * 2));

        spriteBatch.Draw(_pixel, bounds, Color.Gold);
        DrawRectOutline(spriteBatch, bounds, 2, Color.Black);
    }

    private void DrawRectOutline(
        SpriteBatch spriteBatch,
        Rectangle rect,
        int thickness,
        Color color)
    {
        spriteBatch.Draw(_pixel, new Rectangle(rect.Left, rect.Top, rect.Width, thickness), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.Left, rect.Bottom - thickness, rect.Width, thickness), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.Left, rect.Top, thickness, rect.Height), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.Right - thickness, rect.Top, thickness, rect.Height), color);
    }
}