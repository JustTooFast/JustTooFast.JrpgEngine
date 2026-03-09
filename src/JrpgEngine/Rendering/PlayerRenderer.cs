// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.Maps;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JustTooFast.JrpgEngine.Rendering;

public sealed class PlayerRenderer
{
    private readonly Texture2D _pixel;

    public PlayerRenderer(Texture2D pixel)
    {
        _pixel = pixel ?? throw new ArgumentNullException(nameof(pixel));
    }

    public void Draw(
        SpriteBatch spriteBatch,
        Vector2 worldPosition,
        int tileSize)
    {
        if (spriteBatch is null)
        {
            throw new ArgumentNullException(nameof(spriteBatch));
        }

        if (tileSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tileSize), "Tile size must be > 0.");
        }

        var inset = Math.Max(4, tileSize / 8);

        var bounds = new Rectangle(
            (int)worldPosition.X + inset,
            (int)worldPosition.Y + inset,
            tileSize - (inset * 2),
            tileSize - (inset * 2));

        spriteBatch.Begin();

        spriteBatch.Draw(_pixel, bounds, Color.Gold);

        DrawRectOutline(spriteBatch, bounds, 2, Color.Black);

        spriteBatch.End();
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