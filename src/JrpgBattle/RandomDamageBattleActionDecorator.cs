// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

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

        bool wasMiss = _random.NextDouble() < _missChance;
        if (wasMiss)
        {
            return inner;
        }

        int rolledDamage = _random.Next(_minDamage, _maxDamage + 1);

        List<BattleOperation> operations = inner.Operations.ToList();
        operations.Add(new DamageOperation(targetId, rolledDamage));

        return new BattleResolution(operations);
    }
}