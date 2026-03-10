// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Xna.Framework.Graphics;

namespace JustTooFast.JrpgEngine.Rendering;

public interface IMapVisualTextureStore
{
    Texture2D GetRequired(string visualAssetId);
}