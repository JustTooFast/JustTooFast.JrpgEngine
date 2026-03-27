// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

public sealed class RandomEnemyActionChooser : IEnemyActionChooser
{
    private readonly Random _random;

    public RandomEnemyActionChooser(int seed)
    {
        _random = new Random(seed);
    }

    public BattleActionChoice ChooseAction(BattleState state, string actorId)
    {
        if (state is null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        }

        BattleCombatantState actor = state.Combatants.FirstOrDefault(c => c.Id == actorId)
            ?? throw new InvalidOperationException($"Actor '{actorId}' not found.");

        if (actor.IsDefeated)
        {
            throw new InvalidOperationException("Actor is defeated.");
        }

        BattleActionKind actionKind = _random.Next(2) == 0
            ? BattleActionKind.Attack
            : BattleActionKind.Defend;

        string[]? targetIds = null;

        if (actionKind == BattleActionKind.Attack)
        {
            BattleTeam targetTeam = actor.Team == BattleTeam.Party
                ? BattleTeam.Enemy
                : BattleTeam.Party;

            string? targetId = state.Combatants
                .Where(c => c.Team == targetTeam && !c.IsDefeated)
                .Select(c => c.Id)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(targetId))
            {
                targetIds = [targetId];
            }
        }

        return new BattleActionChoice(
            actionKind: actionKind,
            actionId: null,
            targetIds: targetIds);
    }
}