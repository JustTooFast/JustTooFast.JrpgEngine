// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleActionChoice
{
    public BattleActionChoice(
        string actorId,
        BattleActionKind actionKind,
        string? targetId)
    {
        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        }

        bool targetAllowed = actionKind is BattleActionKind.Attack
            or BattleActionKind.Magic
            or BattleActionKind.Item
            or BattleActionKind.Skill;

        bool targetRequired = actionKind == BattleActionKind.Attack;

        if (targetRequired && string.IsNullOrWhiteSpace(targetId))
        {
            throw new ArgumentException("Attack actions require a target.", nameof(targetId));
        }

        if (!targetAllowed && !string.IsNullOrWhiteSpace(targetId))
        {
            throw new ArgumentException("This action kind cannot have a target.", nameof(targetId));
        }

        ActorId = actorId;
        ActionKind = actionKind;
        TargetId = targetId;
    }

    public string ActorId { get; }

    public BattleActionKind ActionKind { get; }

    public string? TargetId { get; }
}