// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

public sealed class AlwaysAttackEnemyActionChooser : IEnemyActionChooser
{
    public BattleActionChoice ChooseAction(BattleChooserContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        BattleState state = context.State;
        string actorId = context.ActorId;

        BattleActorState actor = state.Actors.FirstOrDefault(c => c.Id == actorId)
            ?? throw new InvalidOperationException($"Actor '{actorId}' not found.");

        if (actor.IsDefeated)
        {
            throw new InvalidOperationException("Actor is defeated.");
        }

        BattleTeam targetTeam = actor.Team == BattleTeam.Party
            ? BattleTeam.Enemy
            : BattleTeam.Party;

        string[] availableTargetIds = state.Actors
            .Where(c => c.Team == targetTeam && !c.IsDefeated)
            .Select(c => c.Id)
            .ToArray();

        if (availableTargetIds.Length == 0)
        {
            return new BattleActionChoice(
                actionId: "attack",
                targetMode: BattleTargetMode.None,
                targetIds: null);
        }

        return new BattleActionChoice(
            actionId: "attack",
            targetMode: BattleTargetMode.SingleTarget,
            targetIds: new[] { availableTargetIds[0] });
    }
}