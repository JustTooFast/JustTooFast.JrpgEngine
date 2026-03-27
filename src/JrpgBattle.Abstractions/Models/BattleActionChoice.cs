// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleActionChoice
{
    public BattleActionChoice(
        BattleActionKind actionKind,
        string? actionId,
        IReadOnlyList<string>? targetIds)
    {
        if (targetIds is not null && targetIds.Any(static id => string.IsNullOrWhiteSpace(id)))
        {
            throw new ArgumentException("Target ids cannot contain null or whitespace values.", nameof(targetIds));
        }

        ActionKind = actionKind;
        ActionId = string.IsNullOrWhiteSpace(actionId) ? null : actionId;
        TargetIds = targetIds is null
            ? null
            : new ReadOnlyCollection<string>(targetIds.ToArray());
    }

    public BattleActionKind ActionKind { get; }

    public string? ActionId { get; }

    public IReadOnlyList<string>? TargetIds { get; }
}