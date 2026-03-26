// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

public sealed class EscapeChanceBattleActionDecorator : IBattleActionResolver
{
    private readonly IBattleActionResolver _innerResolver;
    private readonly double _escapeSuccessChance;
    private readonly Random _random;

    public EscapeChanceBattleActionDecorator(
        IBattleActionResolver innerResolver,
        double escapeSuccessChance,
        int seed)
    {
        _innerResolver = innerResolver ?? throw new ArgumentNullException(nameof(innerResolver));

        if (escapeSuccessChance < 0.0 || escapeSuccessChance > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(escapeSuccessChance),
                "Escape success chance must be between 0.0 and 1.0.");
        }

        _escapeSuccessChance = escapeSuccessChance;
        _random = new Random(seed);
    }

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

        if (action.ActionKind != BattleActionKind.Escape)
        {
            return _innerResolver.Resolve(state, action);
        }

        BattleCombatantState actor = state.Combatants.FirstOrDefault(c => c.Id == action.ActorId)
            ?? throw new InvalidOperationException($"Actor '{action.ActorId}' not found.");

        if (!actor.IsAlive)
        {
            throw new InvalidOperationException("Actor is not alive.");
        }

        bool wasEscapeSuccessful = _random.NextDouble() < _escapeSuccessChance;

        return new BattleActionResult(
            actorId: actor.Id,
            targetId: null,
            actionKind: BattleActionKind.Escape,
            damageDealt: 0,
            targetDefeated: false,
            wasMiss: false,
            wasEscapeSuccessful: wasEscapeSuccessful);
    }
}