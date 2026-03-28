// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleMenuOption
{
    public BattleMenuOption(
        string optionId,
        string label,
        BattleMenuOptionKind kind,
        bool isEnabled,
        BattleActionKind? actionKind = null,
        string? actionId = null,
        string? nextContextId = null,
        BattleTargetMode targetMode = BattleTargetMode.None,
        string? disabledReason = null)
    {
        if (string.IsNullOrWhiteSpace(optionId))
        {
            throw new ArgumentException("Option id is required.", nameof(optionId));
        }

        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Option label is required.", nameof(label));
        }

        if (!isEnabled && string.IsNullOrWhiteSpace(disabledReason))
        {
            throw new ArgumentException("Disabled options must provide a disabled reason.", nameof(disabledReason));
        }

        OptionId = optionId;
        Label = label;
        Kind = kind;
        IsEnabled = isEnabled;
        ActionKind = actionKind;
        ActionId = actionId;
        NextContextId = nextContextId;
        TargetMode = targetMode;
        DisabledReason = disabledReason;
    }

    public string OptionId { get; }

    public string Label { get; }

    public BattleMenuOptionKind Kind { get; }

    public bool IsEnabled { get; }

    public BattleActionKind? ActionKind { get; }

    public string? ActionId { get; }

    public string? NextContextId { get; }

    public BattleTargetMode TargetMode { get; }

    public string? DisabledReason { get; }

    public bool RequiresTargetSelection => TargetMode != BattleTargetMode.None;
}
