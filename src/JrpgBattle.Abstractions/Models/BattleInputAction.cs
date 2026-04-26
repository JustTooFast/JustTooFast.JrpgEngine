// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleInputAction
{
    public BattleInputAction(
        string actionId,
        string displayText,
        BattleActionKind actionKind,
        string? category,
        BattleTargetMode targetMode,
        bool isEnabled)
    {
        if (string.IsNullOrWhiteSpace(actionId))
        {
            throw new ArgumentException("Action id is required.", nameof(actionId));
        }

        if (string.IsNullOrWhiteSpace(displayText))
        {
            throw new ArgumentException("Display text is required.", nameof(displayText));
        }

        if (!Enum.IsDefined(actionKind))
        {
            throw new ArgumentOutOfRangeException(nameof(actionKind), "Action kind must be a defined value.");
        }

        if (!Enum.IsDefined(targetMode))
        {
            throw new ArgumentOutOfRangeException(nameof(targetMode), "Target mode must be a defined value.");
        }

        ActionId = actionId;
        DisplayText = displayText;
        ActionKind = actionKind;
        Category = string.IsNullOrWhiteSpace(category) ? null : category;
        TargetMode = targetMode;
        IsEnabled = isEnabled;
    }

    public string ActionId { get; }

    public string DisplayText { get; }

    public BattleActionKind ActionKind { get; }

    public string? Category { get; }

    public BattleTargetMode TargetMode { get; }

    public bool IsEnabled { get; }
}