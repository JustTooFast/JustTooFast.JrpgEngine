// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

public sealed class AlwaysAttackEnemyActionChooser : IEnemyActionChooser
{
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

        string? targetId = state.Combatants
            .Where(c => c.Team == targetTeam && !c.IsDefeated)
            .Select(c => c.Id)
            .FirstOrDefault();

        return new BattleActionChoice(
            actionKind: BattleActionKind.Attack,
            actionId: null,
            targetIds: string.IsNullOrWhiteSpace(targetId)
                ? null
                : new[] { targetId });
    }
}