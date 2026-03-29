// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle.ConsoleHost;

public sealed class RandomDamageBattleActionDecorator : IBattleActionResolver
{
    private readonly IBattleActionResolver _innerResolver;
    private readonly int _minDamage;
    private readonly int _maxDamage;
    private readonly double _missChance;
    private readonly Random _random;

    public RandomDamageBattleActionDecorator(
        IBattleActionResolver innerResolver,
        int minDamage,
        int maxDamage,
        double missChance,
        int seed)
    {
        _innerResolver = innerResolver ?? throw new ArgumentNullException(nameof(innerResolver));

        if (minDamage < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minDamage), "Minimum damage cannot be negative.");
        }

        if (maxDamage < minDamage)
        {
            throw new ArgumentOutOfRangeException(nameof(maxDamage), "Maximum damage cannot be less than minimum damage.");
        }

        if (missChance < 0.0 || missChance > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(missChance), "Miss chance must be between 0.0 and 1.0.");
        }

        _minDamage = minDamage;
        _maxDamage = maxDamage;
        _missChance = missChance;
        _random = new Random(seed);
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

        bool wasMiss = _random.NextDouble() < _missChance;

        var operations = new List<BattleOperation>(inner.Operations.Count);

        foreach (BattleOperation operation in inner.Operations)
        {
            if (operation is DamageOperation damageOperation)
            {
                if (wasMiss)
                {
                    continue;
                }

                int rolledDamage = _random.Next(_minDamage, _maxDamage + 1);
                operations.Add(new DamageOperation(damageOperation.TargetId, rolledDamage));
            }
            else
            {
                operations.Add(operation);
            }
        }

        return new BattleResolution(operations);
    }
}