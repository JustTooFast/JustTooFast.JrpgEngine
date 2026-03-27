// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

public sealed class DefaultBattleActionResolver : IBattleActionResolver
{
    public BattleResolution Resolve(BattleState state, string actorId, BattleActionChoice action)
    {
        if (state is null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        }

        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        BattleCombatantState actor = state.Combatants.FirstOrDefault(c => c.Id == actorId)
            ?? throw new InvalidOperationException($"Actor '{actorId}' not found.");

        if (actor.IsDefeated)
        {
            throw new InvalidOperationException("Actor is defeated.");
        }

        if (action.TargetIds is not null)
        {
            foreach (string targetId in action.TargetIds)
            {
                if (string.IsNullOrWhiteSpace(targetId))
                {
                    throw new ArgumentException("Target id is required.", nameof(action));
                }

                BattleCombatantState target = state.Combatants.FirstOrDefault(c => c.Id == targetId)
                    ?? throw new InvalidOperationException($"Target '{targetId}' not found.");

                if (target.IsDefeated)
                {
                    throw new InvalidOperationException("Target is defeated.");
                }
            }
        }

        return new BattleResolution(Array.Empty<BattleOperation>());
    }
}