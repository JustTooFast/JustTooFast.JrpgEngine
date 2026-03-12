// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.State;
using Microsoft.Xna.Framework;

namespace JustTooFast.JrpgEngine.Rendering;

public sealed class CharacterSpriteSheetLayout
{
    public CharacterSpriteSheetLayout(
        int frameWidth,
        int frameHeight,
        int framesPerDirection)
    {
        if (frameWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frameWidth), "Frame width must be > 0.");
        }

        if (frameHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frameHeight), "Frame height must be > 0.");
        }

        if (framesPerDirection <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(framesPerDirection), "Frames per direction must be > 0.");
        }

        FrameWidth = frameWidth;
        FrameHeight = frameHeight;
        FramesPerDirection = framesPerDirection;
    }

    public int FrameWidth { get; }

    public int FrameHeight { get; }

    public int FramesPerDirection { get; }

    public Rectangle GetSourceRect(FacingDirection facingDirection, int frameIndex)
    {
        if (frameIndex < 0 || frameIndex >= FramesPerDirection)
        {
            throw new ArgumentOutOfRangeException(nameof(frameIndex), "Frame index is out of range.");
        }

        var rowIndex = facingDirection switch
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

        var x = frameIndex * FrameWidth;
        var y = rowIndex * FrameHeight;

        return new Rectangle(x, y, FrameWidth, FrameHeight);
    }
}