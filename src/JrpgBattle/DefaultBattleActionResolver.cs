// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

public sealed class DefaultBattleActionResolver : IBattleActionResolver
{
    public BattleResolution Resolve(BattleResolverContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        BattleState state = context.State;
        string actorId = context.ActorId;
        BattleActionChoice action = context.Action;

        BattleActorState actor = state.Actors.FirstOrDefault(c => c.Id == actorId)
            ?? throw new InvalidOperationException($"Actor '{actorId}' not found.");

        if (actor.IsDefeated)
        {
            throw new InvalidOperationException("Actor is defeated.");
        }

        ValidateActionShape(action);

        return action.ActionKind switch
        {
            BattleActionKind.Attack => ResolveAttack(state, actor, action),
            BattleActionKind.Defend => ResolveDefend(actor),
            BattleActionKind.Escape => ResolveEscape(),
            _ => new BattleResolution(Array.Empty<BattleOperation>())
        };
    }

    private static BattleResolution ResolveAttack(
        BattleState state,
        BattleActorState actor,
        BattleActionChoice action)
    {
        IReadOnlyList<string> targetIds = action.TargetIds ?? Array.Empty<string>();

        if (targetIds.Count == 0)
        {
            return new BattleResolution(Array.Empty<BattleOperation>());
        }

        var operations = new List<BattleOperation>();

        foreach (string targetId in targetIds)
        {
            BattleActorState? target = state.Actors.FirstOrDefault(c => c.Id == targetId);

            if (target is null)
            {
                continue;
            }

            if (target.Team == actor.Team)
            {
                continue;
            }

            if (target.IsDefeated)
            {
                continue;
            }

            operations.Add(new DamageOperation(target.Id, 1));
        }

        return new BattleResolution(operations);
    }

    private static BattleResolution ResolveDefend(BattleActorState actor)
    {
        return new BattleResolution(new BattleOperation[]
        {
            new DefendAppliedOperation(actor.Id)
        });
    }

    private static BattleResolution ResolveEscape()
    {
        return new BattleResolution(Array.Empty<BattleOperation>());
    }

    private static void ValidateActionShape(BattleActionChoice action)
    {
        if (action.TargetIds is null)
        {
            return;
        }

        foreach (string targetId in action.TargetIds)
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                throw new ArgumentException("Target ids cannot contain null or whitespace values.", nameof(action));
            }
        }
    }
}