// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.Maps;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JustTooFast.JrpgEngine.Rendering;

public sealed class DebugMapBackgroundRenderer : IMapBackgroundRenderer
{
    private readonly Texture2D _pixel;

    public DebugMapBackgroundRenderer(Texture2D pixel)
    {
        _pixel = pixel ?? throw new ArgumentNullException(nameof(pixel));
    }

    public void Draw(MapSceneRenderContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var spriteBatch = context.SpriteBatch;
        var mapRuntime = context.RuntimeMap;
        var mapDef = mapRuntime.Definition;
        var tileSize = mapRuntime.TileSize;
        var floorColor = GetFloorColor(mapRuntime);

        for (var y = 0; y < mapDef.Height; y++)
        {
            for (var x = 0; x < mapDef.Width; x++)
            {
                var worldBounds = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);
                var screenBounds = context.WorldToScreen(worldBounds);

                spriteBatch.Draw(_pixel, screenBounds, floorColor);
                DrawRectOutline(spriteBatch, screenBounds, 1, Color.Black * 0.35f);
            }
        }

        foreach (var blockedTile in mapDef.BlockedTiles)
        {
            var blockedWorldBounds = new Rectangle(
                blockedTile.X * tileSize,
                blockedTile.Y * tileSize,
                tileSize,
                tileSize);

            var blockedScreenBounds = context.WorldToScreen(blockedWorldBounds);

            spriteBatch.Draw(_pixel, blockedScreenBounds, Color.DimGray);
            DrawRectOutline(spriteBatch, blockedScreenBounds, 1, Color.Black);
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

    private static Color GetFloorColor(MapRuntime mapRuntime)
    {
        return Color.ForestGreen;
    }
}