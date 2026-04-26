// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleAbilityDefinition
{
    public BattleAbilityDefinition(
        string actionId,
        string displayText,
        string? category,
        BattleTargetMode targetMode,
        BattleExtendedData extendedData)
    {
        if (string.IsNullOrWhiteSpace(actionId))
        {
            throw new ArgumentException("Action id is required.", nameof(actionId));
        }

        if (string.IsNullOrWhiteSpace(displayText))
        {
            throw new ArgumentException("Display text is required.", nameof(displayText));
        }

        if (!Enum.IsDefined(targetMode))
        {
            throw new ArgumentOutOfRangeException(nameof(targetMode), "Target mode must be a defined BattleTargetMode value.");
        }

        if (extendedData is null)
        {
            throw new ArgumentNullException(nameof(extendedData));
        }

        ActionId = actionId;
        DisplayText = displayText;
        Category = string.IsNullOrWhiteSpace(category) ? null : category;
        TargetMode = targetMode;
        ExtendedData = extendedData;
    }

    public string ActionId { get; }

    public string DisplayText { get; }

    public string? Category { get; }

    public BattleTargetMode TargetMode { get; }

    public BattleExtendedData ExtendedData { get; }
}