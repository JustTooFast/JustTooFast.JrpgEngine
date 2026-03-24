// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleActionResult
{
    public BattleActionResult(
        string actorId,
        string? targetId,
        BattleActionKind actionKind,
        int damageDealt,
        bool targetDefeated)
    {
        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        }

        if (actionKind == BattleActionKind.Attack && string.IsNullOrWhiteSpace(targetId))
        {
            throw new ArgumentException("Attack actions require a target.", nameof(targetId));
        }

        if (damageDealt < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(damageDealt), "Damage cannot be negative.");
        }

        ActorId = actorId;
        TargetId = targetId;
        ActionKind = actionKind;
        DamageDealt = damageDealt;
        TargetDefeated = targetDefeated;
    }

    public string ActorId { get; }

    public string? TargetId { get; }

    public BattleActionKind ActionKind { get; }

    public int DamageDealt { get; }

    public bool TargetDefeated { get; }
}