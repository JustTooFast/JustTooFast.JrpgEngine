// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using JustTooFast.JrpgEngine.Definitions;
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
            if (!string.Equals(mapObject.Type, "Npc", StringComparison.Ordinal))
            {
                continue;
            }

            var inset = Math.Max(4, tileSize / 8);

            var bounds = new Rectangle(
                (mapObject.X * tileSize) + inset,
                (mapObject.Y * tileSize) + inset,
                tileSize - (inset * 2),
                tileSize - (inset * 2));

            spriteBatch.Draw(_pixel, bounds, Color.CornflowerBlue);
            DrawRectOutline(spriteBatch, bounds, 2, Color.Black);
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