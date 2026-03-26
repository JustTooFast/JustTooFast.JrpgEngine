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
        bool targetDefeated,
        bool wasMiss,
        bool wasEscapeSuccessful)
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

        if (damageDealt < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(damageDealt), "Damage cannot be negative.");
        }

        bool canMiss = actionKind is BattleActionKind.Attack
            or BattleActionKind.Magic
            or BattleActionKind.Item
            or BattleActionKind.Skill;

        if (wasMiss && !canMiss)
        {
            throw new ArgumentException("This action kind cannot miss.", nameof(wasMiss));
        }

        if (wasMiss && damageDealt != 0)
        {
            throw new ArgumentException("Missed attacks cannot deal damage.", nameof(damageDealt));
        }

        if (wasMiss && targetDefeated)
        {
            throw new ArgumentException("Missed attacks cannot defeat a target.", nameof(targetDefeated));
        }

        if (wasEscapeSuccessful && actionKind != BattleActionKind.Escape)
        {
            throw new ArgumentException("Only escape actions can be marked as successful escapes.", nameof(wasEscapeSuccessful));
        }

        if (actionKind == BattleActionKind.Escape && targetDefeated)
        {
            throw new ArgumentException("Escape actions cannot defeat a target.", nameof(targetDefeated));
        }

        if (actionKind == BattleActionKind.Escape && damageDealt != 0)
        {
            throw new ArgumentException("Escape actions cannot deal damage.", nameof(damageDealt));
        }

        ActorId = actorId;
        TargetId = targetId;
        ActionKind = actionKind;
        DamageDealt = damageDealt;
        TargetDefeated = targetDefeated;
        WasMiss = wasMiss;
        WasEscapeSuccessful = wasEscapeSuccessful;
    }

    public string ActorId { get; }

    public string? TargetId { get; }

    public BattleActionKind ActionKind { get; }

    public int DamageDealt { get; }

    public bool TargetDefeated { get; }

    public bool WasMiss { get; }

    public bool WasEscapeSuccessful { get; }
}