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
        string actionId,
        BattleTargetMode targetMode,
        IReadOnlyList<string>? targetIds)
    {
        if (string.IsNullOrWhiteSpace(actionId))
        {
            throw new ArgumentException("Action id is required.", nameof(actionId));
        }

        if (!Enum.IsDefined(targetMode))
        {
            throw new ArgumentOutOfRangeException(nameof(targetMode), "Target mode must be a defined value.");
        }

        if (targetIds is not null && targetIds.Any(static id => string.IsNullOrWhiteSpace(id)))
        {
            throw new ArgumentException("Target ids cannot contain null or whitespace values.", nameof(targetIds));
        }

        ActionId = actionId;
        TargetMode = targetMode;
        TargetIds = targetIds is null
            ? null
            : new ReadOnlyCollection<string>(targetIds.ToArray());

        ValidateTargetShape(targetMode, TargetIds);
    }

    public string ActionId { get; }

    public BattleTargetMode TargetMode { get; }

    public IReadOnlyList<string>? TargetIds { get; }

    private static void ValidateTargetShape(
        BattleTargetMode targetMode,
        IReadOnlyList<string>? targetIds)
    {
        switch (targetMode)
        {
            case BattleTargetMode.None:
                if (targetIds is not null && targetIds.Count > 0)
                {
                    throw new ArgumentException(
                        "Choices with target mode None must not provide target ids.",
                        nameof(targetIds));
                }

                break;

            case BattleTargetMode.SingleTarget:
                if (targetIds is not null && targetIds.Count != 1)
                {
                    throw new ArgumentException(
                        "Choices with target mode SingleTarget must provide either no target ids yet or exactly one target id.",
                        nameof(targetIds));
                }

                break;

            case BattleTargetMode.AllAllies:
            case BattleTargetMode.AllEnemies:
                if (targetIds is not null && targetIds.Count > 0)
                {
                    throw new ArgumentException(
                        $"Choices with target mode '{targetMode}' must not provide explicit target ids.",
                        nameof(targetIds));
                }

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(targetMode), targetMode, "Unsupported target mode.");
        }
    }
}