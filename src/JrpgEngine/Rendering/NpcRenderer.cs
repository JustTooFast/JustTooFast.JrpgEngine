// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.Maps;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JustTooFast.JrpgEngine.Rendering;

public sealed class NpcRenderer
{
    private readonly Texture2D _pixel;

    public NpcRenderer(Texture2D pixel)
    {
        _pixel = pixel ?? throw new ArgumentNullException(nameof(pixel));
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

        var mapDef = mapRuntime.Definition;
        var tileSize = mapRuntime.TileSize;

        spriteBatch.Begin();

        foreach (var mapObject in mapDef.Objects)
        {
            if (string.Equals(mapObject.Type, "Npc", StringComparison.Ordinal))
            {
                DrawNpc(spriteBatch, mapObject.X, mapObject.Y, tileSize);
                continue;
            }

            if (string.Equals(mapObject.Type, "Chest", StringComparison.Ordinal))
            {
                DrawChest(spriteBatch, mapObject.X, mapObject.Y, tileSize);
            }
        }

        spriteBatch.End();
    }

    private void DrawNpc(SpriteBatch spriteBatch, int tileX, int tileY, int tileSize)
    {
        var inset = Math.Max(4, tileSize / 8);

        var bounds = new Rectangle(
            (tileX * tileSize) + inset,
            (tileY * tileSize) + inset,
            tileSize - (inset * 2),
            tileSize - (inset * 2));

        spriteBatch.Draw(_pixel, bounds, Color.CornflowerBlue);
        DrawRectOutline(spriteBatch, bounds, 2, Color.Black);
    }

    private void DrawChest(SpriteBatch spriteBatch, int tileX, int tileY, int tileSize)
    {
        var insetX = Math.Max(3, tileSize / 10);
        var insetY = Math.Max(6, tileSize / 5);

        var bounds = new Rectangle(
            (tileX * tileSize) + insetX,
            (tileY * tileSize) + insetY,
            tileSize - (insetX * 2),
            tileSize - (insetY + insetX));

        spriteBatch.Draw(_pixel, bounds, Color.SaddleBrown);
        DrawRectOutline(spriteBatch, bounds, 2, Color.Black);

        var bandHeight = Math.Max(2, tileSize / 8);
        var bandY = bounds.Y + (bounds.Height / 2) - (bandHeight / 2);
        var band = new Rectangle(bounds.X, bandY, bounds.Width, bandHeight);
        spriteBatch.Draw(_pixel, band, Color.Gold);

        var latchSize = Math.Max(2, tileSize / 8);
        var latch = new Rectangle(
            bounds.X + (bounds.Width / 2) - (latchSize / 2),
            bandY - 1,
            latchSize,
            bandHeight + 2);
        spriteBatch.Draw(_pixel, latch, Color.Black);
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