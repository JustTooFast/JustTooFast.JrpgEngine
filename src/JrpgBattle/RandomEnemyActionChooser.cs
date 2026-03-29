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

        BattleActorDefinition actorDefinition = context.Definition.PartyActors
            .Concat(context.Definition.EnemyActors)
            .FirstOrDefault(a => a.Id == actorId)
            ?? throw new InvalidOperationException($"Actor definition '{actorId}' not found.");

        BattleTeam targetTeam = actor.Team == BattleTeam.Party
            ? BattleTeam.Enemy
            : BattleTeam.Party;

        string[] availableTargetIds = state.Actors
            .Where(c => c.Team == targetTeam && !c.IsDefeated)
            .Select(c => c.Id)
            .ToArray();

        bool canAttack = actorDefinition.AllowedActions.Any(a => a.ActionKind == BattleActionKind.Attack)
            && availableTargetIds.Length > 0;

        bool canDefend = actorDefinition.AllowedActions.Any(a => a.ActionKind == BattleActionKind.Defend);

        if (canAttack && canDefend)
        {
            bool chooseAttack = _random.Next(2) == 0;

            if (chooseAttack)
            {
                string targetId = availableTargetIds[_random.Next(availableTargetIds.Length)];

                return new BattleActionChoice(
                    actionKind: BattleActionKind.Attack,
                    actionId: null,
                    targetMode: BattleTargetMode.SingleTarget,
                    targetIds: new[] { targetId });
            }

            return new BattleActionChoice(
                actionKind: BattleActionKind.Defend,
                actionId: null,
                targetMode: BattleTargetMode.None,
                targetIds: null);
        }

        if (canAttack)
        {
            string targetId = availableTargetIds[_random.Next(availableTargetIds.Length)];

            return new BattleActionChoice(
                actionKind: BattleActionKind.Attack,
                actionId: null,
                targetMode: BattleTargetMode.SingleTarget,
                targetIds: new[] { targetId });
        }

        if (canDefend)
        {
            return new BattleActionChoice(
                actionKind: BattleActionKind.Defend,
                actionId: null,
                targetMode: BattleTargetMode.None,
                targetIds: null);
        }

        if (actorDefinition.AllowedActions.Any(a => a.ActionKind == BattleActionKind.Wait))
        {
            return new BattleActionChoice(
                actionKind: BattleActionKind.Wait,
                actionId: null,
                targetMode: BattleTargetMode.None,
                targetIds: null);
        }

        throw new InvalidOperationException(
            $"Actor '{actorId}' has no supported allowed actions for {nameof(RandomEnemyActionChooser)}.");
    }
}