// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

public sealed class FixedDamageBattleActionDecorator : IBattleActionResolver
{
    private readonly IBattleActionResolver _innerResolver;
    private readonly int _damage;

    public FixedDamageBattleActionDecorator(
        IBattleActionResolver innerResolver,
        int damage)
    {
        _innerResolver = innerResolver ?? throw new ArgumentNullException(nameof(innerResolver));

        if (damage <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(damage), "Damage must be greater than zero.");
        }

        _damage = damage;
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

        if (action.ActionKind != BattleActionKind.Attack)
        {
            return inner;
        }

        string? targetId = action.TargetIds?.Count > 0
            ? action.TargetIds[0]
            : null;

        if (string.IsNullOrWhiteSpace(targetId))
        {
            return inner;
        }

        List<BattleOperation> operations = inner.Operations.ToList();
        operations.Add(new DamageOperation(targetId, _damage));

        return new BattleResolution(operations);
    }
}