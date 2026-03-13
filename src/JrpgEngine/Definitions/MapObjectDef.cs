// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

namespace JustTooFast.JrpgEngine.Definitions;

public sealed class MapObjectDef
{
    public string Id { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public bool BlocksMovement { get; set; } = true;

    public string? VisualId { get; set; }
}