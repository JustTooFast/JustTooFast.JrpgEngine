// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace JustTooFast.JrpgEngine.Rendering;

public sealed class VisualTextureStore : IVisualTextureStore
{
    private readonly ContentManager _content;
    private readonly Dictionary<string, Texture2D> _cache = new(StringComparer.Ordinal);

    public VisualTextureStore(ContentManager content)
    {
        _content = content ?? throw new ArgumentNullException(nameof(content));
    }

    public Texture2D GetRequired(string visualAssetId)
    {
        if (string.IsNullOrWhiteSpace(visualAssetId))
        {
            throw new ArgumentException("Visual asset id cannot be null or empty.", nameof(visualAssetId));
        }

        if (_cache.TryGetValue(visualAssetId, out var texture))
        {
            return texture;
        }

        texture = _content.Load<Texture2D>(visualAssetId);
        _cache.Add(visualAssetId, texture);
        return texture;
    }
}