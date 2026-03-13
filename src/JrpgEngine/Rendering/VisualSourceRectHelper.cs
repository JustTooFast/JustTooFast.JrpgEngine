// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.Definitions;
using JustTooFast.JrpgEngine.State;
using Microsoft.Xna.Framework;

namespace JustTooFast.JrpgEngine.Rendering;

public static class VisualSourceRectHelper
{
    public static Rectangle GetSourceRect(VisualDef visualDef, int row, int frameIndex)
    {
        if (visualDef is null)
        {
            throw new ArgumentNullException(nameof(visualDef));
        }

        var facingDirections = visualDef.FacingDirections ?? 1;

        if (row < 0 || row >= facingDirections)
        {
            throw new ArgumentOutOfRangeException(nameof(row), "Row is out of range.");
        }

        if (frameIndex < 0 || frameIndex >= visualDef.FrameCount)
        {
            throw new ArgumentOutOfRangeException(nameof(frameIndex), "Frame index is out of range.");
        }

        var x = frameIndex * visualDef.FrameWidth;
        var y = row * visualDef.FrameHeight;

        return new Rectangle(
            x,
            y,
            visualDef.FrameWidth,
            visualDef.FrameHeight);
    }

    public static int GetFacingRow(VisualDef visualDef, FacingDirection facingDirection)
    {
        if (visualDef is null)
        {
            throw new ArgumentNullException(nameof(visualDef));
        }

        var facingDirections = visualDef.FacingDirections ?? 1;
        if (facingDirections == 1)
        {
            return 0;
        }

        if (facingDirections != 4)
        {
            throw new InvalidOperationException(
                $"Unsupported FacingDirections value '{facingDirections}'.");
        }

        return facingDirection switch
        {
            FacingDirection.Down => 0,
            FacingDirection.Left => 1,
            FacingDirection.Right => 2,
            FacingDirection.Up => 3,
            _ => throw new ArgumentOutOfRangeException(
                nameof(facingDirection),
                facingDirection,
                "Unsupported facing direction.")
        };
    }
}