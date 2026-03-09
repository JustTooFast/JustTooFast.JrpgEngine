// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

namespace JustTooFast.JrpgEngine.Definitions;

public sealed class InteractionResultDef
{
    public string Type { get; set; } = string.Empty;

    public string? FlagId { get; set; }

    public string? ItemId { get; set; }

    public int Amount { get; set; } = 1;
}