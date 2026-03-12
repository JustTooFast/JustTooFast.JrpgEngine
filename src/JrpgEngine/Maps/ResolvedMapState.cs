// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using JustTooFast.JrpgEngine.Definitions;

namespace JustTooFast.JrpgEngine.Maps;

public sealed class ResolvedMapState
{
    public ResolvedMapState(
        MapDef effectiveMapDef,
        string? activeVariantId,
        string? visualAssetId,
        string? overheadVisualAssetId)
    {
        EffectiveMapDef = effectiveMapDef;
        ActiveVariantId = activeVariantId;
        VisualAssetId = visualAssetId;
        OverheadVisualAssetId = overheadVisualAssetId;
    }

    public MapDef EffectiveMapDef { get; }

    public string? ActiveVariantId { get; }

    public string? VisualAssetId { get; }

    public string? OverheadVisualAssetId { get; }
}