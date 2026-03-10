// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using JustTooFast.JrpgEngine.Definitions;

namespace JustTooFast.JrpgEngine.Maps;

public sealed class ResolvedMapState
{
    public ResolvedMapState(
        MapDef effectiveMapDef,
        string? activeVariantId,
        string? visualStyleId)
    {
        EffectiveMapDef = effectiveMapDef;
        ActiveVariantId = activeVariantId;
        VisualStyleId = visualStyleId;
    }

    public MapDef EffectiveMapDef { get; }

    public string? ActiveVariantId { get; }

    public string? VisualStyleId { get; }
}