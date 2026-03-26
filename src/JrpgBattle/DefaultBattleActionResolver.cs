// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

public sealed class DefaultBattleActionResolver : IBattleActionResolver
{
    public BattleActionResult Resolve(BattleState state, BattleActionChoice action)
    {
        if (state is null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        BattleCombatantState actor = state.Combatants.FirstOrDefault(c => c.Id == action.ActorId)
            ?? throw new InvalidOperationException($"Actor '{action.ActorId}' not found.");

        if (!actor.IsAlive)
        {
            throw new InvalidOperationException("Actor is not alive.");
        }

        switch (action.ActionKind)
        {
            case BattleActionKind.Attack:
                return ResolveTargetedNoOp(state, actor, action, wasMiss: false);

            case BattleActionKind.Magic:
                return ResolveTargetedNoOp(state, actor, action, wasMiss: false);

            case BattleActionKind.Item:
                return ResolveTargetedNoOp(state, actor, action, wasMiss: false);

            case BattleActionKind.Skill:
                return ResolveTargetedNoOp(state, actor, action, wasMiss: false);

            case BattleActionKind.Defend:
                return new BattleActionResult(
                    actorId: actor.Id,
                    targetId: null,
                    actionKind: BattleActionKind.Defend,
                    damageDealt: 0,
                    targetDefeated: false,
                    wasMiss: false,
                    wasEscapeSuccessful: false);

            case BattleActionKind.Escape:
                return new BattleActionResult(
                    actorId: actor.Id,
                    targetId: null,
                    actionKind: BattleActionKind.Escape,
                    damageDealt: 0,
                    targetDefeated: false,
                    wasMiss: false,
                    wasEscapeSuccessful: true);

            case BattleActionKind.Wait:
                return new BattleActionResult(
                    actorId: actor.Id,
                    targetId: null,
                    actionKind: BattleActionKind.Wait,
                    damageDealt: 0,
                    targetDefeated: false,
                    wasMiss: false,
                    wasEscapeSuccessful: false);

            default:
                throw new NotSupportedException($"Action '{action.ActionKind}' is not supported.");
        }
    }

    private static BattleActionResult ResolveTargetedNoOp(
        BattleState state,
        BattleCombatantState actor,
        BattleActionChoice action,
        bool wasMiss)
    {
        if (string.IsNullOrWhiteSpace(action.TargetId))
        {
            return new BattleActionResult(
                actorId: actor.Id,
                targetId: null,
                actionKind: action.ActionKind,
                damageDealt: 0,
                targetDefeated: false,
                wasMiss: wasMiss,
                wasEscapeSuccessful: false);
        }

        BattleCombatantState target = state.Combatants.FirstOrDefault(c => c.Id == action.TargetId)
            ?? throw new InvalidOperationException($"Target '{action.TargetId}' not found.");

        if (!target.IsAlive)
        {
            throw new InvalidOperationException("Target is not alive.");
        }

        return new BattleActionResult(
            actorId: actor.Id,
            targetId: target.Id,
            actionKind: action.ActionKind,
            damageDealt: 0,
            targetDefeated: false,
            wasMiss: wasMiss,
            wasEscapeSuccessful: false);
    }
}