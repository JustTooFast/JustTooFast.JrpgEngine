// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle.Tests.Fakes;

public sealed class FixedDamageBattleActionDecorator : IBattleActionResolver
{
    private readonly IBattleActionResolver _innerResolver;
    private readonly int _damage;

    public FixedDamageBattleActionDecorator(
        IBattleActionResolver innerResolver,
        int damage)
    {
        _innerResolver = innerResolver ?? throw new ArgumentNullException(nameof(innerResolver));

        if (damage < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(damage), "Damage cannot be negative.");
        }

        _damage = damage;
    }

    public BattleResolution Resolve(BattleResolverContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        BattleResolution inner = _innerResolver.Resolve(context);

        if (context.Action.ActionKind != BattleActionKind.Attack)
        {
            return inner;
        }

        var operations = new List<BattleOperation>(inner.Operations.Count);

        foreach (BattleOperation operation in inner.Operations)
        {
            if (operation is DamageOperation damageOperation)
            {
                operations.Add(new DamageOperation(damageOperation.TargetId, _damage));
            }
            else
            {
                operations.Add(operation);
            }
        }

        return new BattleResolution(operations);
    }
}