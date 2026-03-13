// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

namespace JustTooFast.JrpgEngine.Definitions;

public sealed class VisualDef
{
    public string Id { get; set; } = string.Empty;

    public string VisualAssetId { get; set; } = string.Empty;

    public int FrameWidth { get; set; }

    public int FrameHeight { get; set; }

    public int FrameCount { get; set; }

    public int? FacingDirections { get; set; }
}