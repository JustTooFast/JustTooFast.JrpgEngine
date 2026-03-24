// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

public sealed class BattleRuntime : IBattleRuntime
{
    private readonly IBattleActionResolver _actionResolver;
    private readonly IBattleRewardCalculator _rewardCalculator;

    public BattleRuntime(
        IBattleActionResolver actionResolver,
        IBattleRewardCalculator rewardCalculator)
    {
        _actionResolver = actionResolver ?? throw new ArgumentNullException(nameof(actionResolver));
        _rewardCalculator = rewardCalculator ?? throw new ArgumentNullException(nameof(rewardCalculator));
    }

    public BattleState Initialize(BattleDefinition definition)
    {
        if (definition is null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        var combatants = new List<BattleCombatantState>();

        foreach (var c in definition.PartyCombatants)
        {
            combatants.Add(new BattleCombatantState(
                c.Id,
                c.Name,
                c.Team,
                c.MaxHp,
                c.MaxHp));
        }

        foreach (var c in definition.EnemyCombatants)
        {
            combatants.Add(new BattleCombatantState(
                c.Id,
                c.Name,
                c.Team,
                c.MaxHp,
                c.MaxHp));
        }

        return new BattleState(
            combatants,
            isEnded: false,
            outcome: BattleOutcome.None);
    }

    public BattleState ApplyAction(BattleState state, BattleActionChoice action)
    {
        if (state is null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (state.IsEnded)
        {
            return state;
        }

        var result = _actionResolver.Resolve(state, action);

        var updatedCombatants = state.Combatants
            .Select(c =>
            {
                if (c.Id != result.TargetId)
                {
                    return c;
                }

                var newHp = Math.Max(0, c.CurrentHp - result.DamageDealt);

                return new BattleCombatantState(
                    c.Id,
                    c.Name,
                    c.Team,
                    newHp,
                    c.MaxHp);
            })
            .ToList();

        var newState = new BattleState(
            updatedCombatants,
            isEnded: false,
            outcome: BattleOutcome.None);

        return EvaluateEnd(newState);
    }

    public BattleResult GetResult(BattleState state)
    {
        if (state is null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        if (!state.IsEnded)
        {
            throw new InvalidOperationException("Battle has not ended.");
        }

        BattleReward? reward = null;

        if (state.Outcome == BattleOutcome.Victory)
        {
            reward = _rewardCalculator.Calculate(state);
        }

        return new BattleResult(state.Outcome, reward);
    }

    private static BattleState EvaluateEnd(BattleState state)
    {
        var partyAlive = state.Combatants.Any(c => c.Team == BattleTeam.Party && c.IsAlive);
        var enemiesAlive = state.Combatants.Any(c => c.Team == BattleTeam.Enemy && c.IsAlive);

        if (partyAlive && enemiesAlive)
        {
            return state;
        }

        var outcome = partyAlive
            ? BattleOutcome.Victory
            : BattleOutcome.Defeat;

        return new BattleState(
            state.Combatants,
            isEnded: true,
            outcome: outcome);
    }
}