// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.Maps;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JustTooFast.JrpgEngine.Rendering;

public sealed class MapObjectRenderer
{
    private readonly Texture2D _pixel;

    public MapObjectRenderer(Texture2D pixel)
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
                continue;
            }

            if (string.Equals(mapObject.Type, "LockedDoor", StringComparison.Ordinal) ||
                string.Equals(mapObject.Type, "FlagGate", StringComparison.Ordinal))
            {
                DrawGate(spriteBatch, mapObject.X, mapObject.Y, tileSize);
                continue;
            }

            if (string.Equals(mapObject.Type, "MapExit", StringComparison.Ordinal))
            {
                DrawExit(spriteBatch, mapObject.X, mapObject.Y, tileSize);
            }
        }
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

    private void DrawGate(SpriteBatch spriteBatch, int tileX, int tileY, int tileSize)
    {
        var inset = Math.Max(2, tileSize / 12);

        var bounds = new Rectangle(
            (tileX * tileSize) + inset,
            (tileY * tileSize) + inset,
            tileSize - (inset * 2),
            tileSize - (inset * 2));

        spriteBatch.Draw(_pixel, bounds, Color.DarkRed);
        DrawRectOutline(spriteBatch, bounds, 2, Color.Black);

        var barWidth = Math.Max(2, tileSize / 8);
        var leftBar = new Rectangle(bounds.X + (bounds.Width / 3) - (barWidth / 2), bounds.Y, barWidth, bounds.Height);
        var rightBar = new Rectangle(bounds.X + ((bounds.Width * 2) / 3) - (barWidth / 2), bounds.Y, barWidth, bounds.Height);

        spriteBatch.Draw(_pixel, leftBar, Color.Black);
        spriteBatch.Draw(_pixel, rightBar, Color.Black);
    }

    private void DrawExit(SpriteBatch spriteBatch, int tileX, int tileY, int tileSize)
    {
        var inset = Math.Max(4, tileSize / 8);

        var bounds = new Rectangle(
            (tileX * tileSize) + inset,
            (tileY * tileSize) + inset,
            tileSize - (inset * 2),
            tileSize - (inset * 2));

        spriteBatch.Draw(_pixel, bounds, Color.MediumPurple);
        DrawRectOutline(spriteBatch, bounds, 2, Color.Black);

        var innerInset = Math.Max(4, tileSize / 6);
        var inner = new Rectangle(
            bounds.X + innerInset,
            bounds.Y + innerInset,
            bounds.Width - (innerInset * 2),
            bounds.Height - (innerInset * 2));

        if (inner.Width > 0 && inner.Height > 0)
        {
            spriteBatch.Draw(_pixel, inner, Color.Black);
        }
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