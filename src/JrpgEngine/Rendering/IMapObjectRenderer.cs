// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using JustTooFast.JrpgEngine.Maps;
using Microsoft.Xna.Framework.Graphics;

namespace JustTooFast.JrpgEngine.Rendering;

public interface IMapObjectRenderer
{
    void Draw(SpriteBatch spriteBatch, MapRuntime mapRuntime);
}