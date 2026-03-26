// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

public sealed class FixedDamageBattleActionDecorator : IBattleActionResolver
{
    private readonly IBattleActionResolver _innerResolver;
    private readonly int _damage;

    public FixedDamageBattleActionDecorator(
        IBattleActionResolver innerResolver,
        int damage)
    {
        _innerResolver = innerResolver ?? throw new ArgumentNullException(nameof(innerResolver));

        if (damage <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(damage), "Damage must be greater than zero.");
        }

        _damage = damage;
    }

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

        if (action.ActionKind != BattleActionKind.Attack)
        {
            return _innerResolver.Resolve(state, action);
        }

        BattleCombatantState actor = state.Combatants.FirstOrDefault(c => c.Id == action.ActorId)
            ?? throw new InvalidOperationException($"Actor '{action.ActorId}' not found.");

        if (!actor.IsAlive)
        {
            throw new InvalidOperationException("Actor is not alive.");
        }

        BattleCombatantState target = state.Combatants.FirstOrDefault(c => c.Id == action.TargetId)
            ?? throw new InvalidOperationException($"Target '{action.TargetId}' not found.");

        if (!target.IsAlive)
        {
            throw new InvalidOperationException("Target is not alive.");
        }

        if (actor.Team == target.Team)
        {
            throw new InvalidOperationException("Cannot target a combatant on the same team.");
        }

        int damage = Math.Min(_damage, target.CurrentHp);
        bool targetDefeated = target.CurrentHp - damage <= 0;

        return new BattleActionResult(
            actorId: actor.Id,
            targetId: target.Id,
            actionKind: BattleActionKind.Attack,
            damageDealt: damage,
            targetDefeated: targetDefeated,
            wasMiss: false,
            wasEscapeSuccessful: false);
    }
}