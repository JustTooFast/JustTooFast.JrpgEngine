// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JustTooFast.JrpgEngine.Rendering;

public static class DebugTextureFactory
{
    public static Texture2D CreateSolidTexture(GraphicsDevice graphicsDevice, Color color)
    {
        var texture = new Texture2D(graphicsDevice, 1, 1);
        texture.SetData(new[] { color });
        return texture;
    }
}