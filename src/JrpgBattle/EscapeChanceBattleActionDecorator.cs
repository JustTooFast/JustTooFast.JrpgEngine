// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
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

        BattleResolution inner = _innerResolver.Resolve(state, actorId, action);

        if (action.ActionKind != BattleActionKind.Escape)
        {
            return inner;
        }

        bool wasEscapeSuccessful = _random.NextDouble() < _escapeSuccessChance;

        List<BattleOperation> operations = inner.Operations.ToList();
        operations.Add(
            wasEscapeSuccessful
                ? new EscapeSucceededOperation()
                : new EscapeFailedOperation());

        return new BattleResolution(operations);
    }
}