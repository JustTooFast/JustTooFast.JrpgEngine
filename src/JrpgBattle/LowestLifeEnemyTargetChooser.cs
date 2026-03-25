// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

public sealed class LowestLifeEnemyTargetChooser : IEnemyTargetChooser
{
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

        BattleCombatantState target = state.Combatants
            .Where(c => c.Team == targetTeam && c.IsAlive)
            .OrderBy(c => c.CurrentHp)
            .ThenBy(c => c.Id)
            .FirstOrDefault()
            ?? throw new InvalidOperationException("No valid target found.");

        return target.Id;
    }
}