// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace JustTooFast.JrpgEngine.Definitions;

public sealed class MapStateVariantDef
{
    public string Id { get; set; } = string.Empty;

    public string FlagId { get; set; } = string.Empty;

    public bool ActiveWhenSet { get; set; } = true;

    public string? VisualAssetOverrideId { get; set; }

    public string? OverheadVisualAssetOverrideId { get; set; }

    public List<string> EnableObjectIds { get; set; } = new();

    public List<string> DisableObjectIds { get; set; } = new();
}