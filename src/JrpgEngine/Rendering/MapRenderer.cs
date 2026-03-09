// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.Definitions;
using JustTooFast.JrpgEngine.Maps;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JustTooFast.JrpgEngine.Rendering;

public sealed class MapRenderer
{
    private readonly Texture2D _pixel;

    public MapRenderer(Texture2D pixel)
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

        for (var y = 0; y < mapDef.Height; y++)
        {
            for (var x = 0; x < mapDef.Width; x++)
            {
                var bounds = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);

                spriteBatch.Draw(_pixel, bounds, Color.ForestGreen);

                var borderThickness = 1;
                DrawRectOutline(spriteBatch, bounds, borderThickness, Color.Black * 0.35f);
            }
        }

        foreach (var blockedTile in mapDef.BlockedTiles)
        {
            var blockedBounds = new Rectangle(
                blockedTile.X * tileSize,
                blockedTile.Y * tileSize,
                tileSize,
                tileSize);

            spriteBatch.Draw(_pixel, blockedBounds, Color.DimGray);
            DrawRectOutline(spriteBatch, blockedBounds, 1, Color.Black);
        }

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