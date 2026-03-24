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

        if (actionKind == BattleActionKind.Attack && string.IsNullOrWhiteSpace(targetId))
        {
            throw new ArgumentException("Attack actions require a target.", nameof(targetId));
        }

        ActorId = actorId;
        ActionKind = actionKind;
        TargetId = targetId;
    }

    public string ActorId { get; }

    public BattleActionKind ActionKind { get; }

    public string? TargetId { get; }
}