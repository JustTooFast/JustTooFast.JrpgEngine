// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

public sealed class RandomDamageBattleActionDecorator : IBattleActionResolver
{
    private readonly IBattleActionResolver _innerResolver;
    private readonly int _minDamage;
    private readonly int _maxDamage;
    private readonly double _missChance;
    private readonly Random _random;

    public RandomDamageBattleActionDecorator(
        IBattleActionResolver innerResolver,
        int minDamage,
        int maxDamage,
        double missChance,
        int seed)
    {
        _innerResolver = innerResolver ?? throw new ArgumentNullException(nameof(innerResolver));

        if (minDamage < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minDamage), "Minimum damage cannot be negative.");
        }

        if (maxDamage < minDamage)
        {
            throw new ArgumentOutOfRangeException(nameof(maxDamage), "Maximum damage cannot be less than minimum damage.");
        }

        if (missChance < 0.0 || missChance > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(missChance), "Miss chance must be between 0.0 and 1.0.");
        }

        _minDamage = minDamage;
        _maxDamage = maxDamage;
        _missChance = missChance;
        _random = new Random(seed);
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

        bool wasMiss = _random.NextDouble() < _missChance;
        if (wasMiss)
        {
            return new BattleActionResult(
                actorId: actor.Id,
                targetId: target.Id,
                actionKind: BattleActionKind.Attack,
                damageDealt: 0,
                targetDefeated: false,
                wasMiss: true,
                wasEscapeSuccessful: false);
        }

        int rolledDamage = _random.Next(_minDamage, _maxDamage + 1);
        int appliedDamage = Math.Min(rolledDamage, target.CurrentHp);
        bool targetDefeated = target.CurrentHp - appliedDamage <= 0;

        return new BattleActionResult(
            actorId: actor.Id,
            targetId: target.Id,
            actionKind: BattleActionKind.Attack,
            damageDealt: appliedDamage,
            targetDefeated: targetDefeated,
            wasMiss: false,
            wasEscapeSuccessful: false);
    }
}