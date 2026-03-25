// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

public sealed class RandomEnemyTargetChooser : IEnemyTargetChooser
{
    private readonly Random _random;

    public RandomEnemyTargetChooser(int seed)
    {
        _random = new Random(seed);
    }

    public string ChooseTargetId(BattleState state, string actorId)
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

        if (!actor.IsAlive)
        {
            throw new InvalidOperationException("Actor is not alive.");
        }

        BattleTeam targetTeam = actor.Team == BattleTeam.Party
            ? BattleTeam.Enemy
            : BattleTeam.Party;

        var validTargets = state.Combatants
            .Where(c => c.Team == targetTeam && c.IsAlive)
            .ToList();

        if (validTargets.Count == 0)
        {
            throw new InvalidOperationException("No valid target found.");
        }

        int index = _random.Next(validTargets.Count);

        return validTargets[index].Id;
    }
}