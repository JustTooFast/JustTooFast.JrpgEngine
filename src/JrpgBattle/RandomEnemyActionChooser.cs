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

        BattleTeam targetTeam = actor.Team == BattleTeam.Party
            ? BattleTeam.Enemy
            : BattleTeam.Party;

        string[] availableTargetIds = state.Combatants
            .Where(c => c.Team == targetTeam && !c.IsDefeated)
            .Select(c => c.Id)
            .ToArray();

        bool canAttack = availableTargetIds.Length > 0;
        bool chooseAttack = canAttack && _random.Next(2) == 0;

        if (chooseAttack)
        {
            string targetId = availableTargetIds[_random.Next(availableTargetIds.Length)];

            return new BattleActionChoice(
                actionKind: BattleActionKind.Attack,
                actionId: null,
                targetIds: new[] { targetId });
        }

        return new BattleActionChoice(
            actionKind: BattleActionKind.Defend,
            actionId: null,
            targetIds: null);
    }
}