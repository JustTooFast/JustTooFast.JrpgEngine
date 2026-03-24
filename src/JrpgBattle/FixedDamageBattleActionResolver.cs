// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

public sealed class FixedDamageBattleActionResolver : IBattleActionResolver
{
    private const int FixedDamage = 5;

    public BattleActionResult Resolve(BattleState state, BattleActionChoice action)
    {
        if (state is null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var actor = state.Combatants.FirstOrDefault(c => c.Id == action.ActorId);
        if (actor is null)
        {
            throw new InvalidOperationException($"Actor '{action.ActorId}' not found.");
        }

        if (!actor.IsAlive)
        {
            throw new InvalidOperationException("Actor is not alive.");
        }

        if (action.ActionKind != BattleActionKind.Attack)
        {
            throw new NotSupportedException($"Action '{action.ActionKind}' is not supported in v0.");
        }

        var target = state.Combatants.FirstOrDefault(c => c.Id == action.TargetId);
        if (target is null)
        {
            throw new InvalidOperationException($"Target '{action.TargetId}' not found.");
        }

        if (!target.IsAlive)
        {
            throw new InvalidOperationException("Target is not alive.");
        }

        if (actor.Team == target.Team)
        {
            throw new InvalidOperationException("Cannot target a combatant on the same team.");
        }

        var damage = Math.Min(FixedDamage, target.CurrentHp);
        var targetDefeated = target.CurrentHp - damage <= 0;

        return new BattleActionResult(
            actor.Id,
            target.Id,
            action.ActionKind,
            damage,
            targetDefeated);
    }
}