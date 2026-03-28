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
        BattleTargetMode targetMode,
        IReadOnlyList<string>? targetIds)
    {
        if (actionKind == BattleActionKind.Attack && string.IsNullOrWhiteSpace(actionId) == false)
        {
            throw new ArgumentException("Attack choices must not provide a specific action id.", nameof(actionId));
        }

        if (actionKind == BattleActionKind.Defend && string.IsNullOrWhiteSpace(actionId) == false)
        {
            throw new ArgumentException("Defend choices must not provide a specific action id.", nameof(actionId));
        }

        if (actionKind == BattleActionKind.Escape && string.IsNullOrWhiteSpace(actionId) == false)
        {
            throw new ArgumentException("Escape choices must not provide a specific action id.", nameof(actionId));
        }

        if (actionKind == BattleActionKind.Wait && string.IsNullOrWhiteSpace(actionId) == false)
        {
            throw new ArgumentException("Wait choices must not provide a specific action id.", nameof(actionId));
        }

        if ((actionKind == BattleActionKind.Magic
            || actionKind == BattleActionKind.Skill
            || actionKind == BattleActionKind.Item)
            && string.IsNullOrWhiteSpace(actionId))
        {
            throw new ArgumentException(
                $"Choices of kind '{actionKind}' must provide a specific action id.",
                nameof(actionId));
        }

        if (targetIds is not null && targetIds.Any(static id => string.IsNullOrWhiteSpace(id)))
        {
            throw new ArgumentException("Target ids cannot contain null or whitespace values.", nameof(targetIds));
        }

        ActionKind = actionKind;
        ActionId = string.IsNullOrWhiteSpace(actionId) ? null : actionId;
        TargetMode = targetMode;
        TargetIds = targetIds is null
            ? null
            : new ReadOnlyCollection<string>(targetIds.ToArray());

        ValidateTargetShape(targetMode, TargetIds);
    }

    public BattleActionKind ActionKind { get; }

    public string? ActionId { get; }

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