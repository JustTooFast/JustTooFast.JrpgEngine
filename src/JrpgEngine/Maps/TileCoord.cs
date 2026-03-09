// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

namespace JustTooFast.JrpgEngine.Maps;

public readonly record struct TileCoord(int X, int Y)
{
    public static TileCoord operator +(TileCoord left, TileCoord right)
    {
        return new TileCoord(left.X + right.X, left.Y + right.Y);
    }
}