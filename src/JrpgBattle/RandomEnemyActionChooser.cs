// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
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

    public BattleActionKind ChooseAction(BattleState state, string actorId)
    {
        if (state is null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        }

        return _random.Next(2) == 0
            ? BattleActionKind.Attack
            : BattleActionKind.Defend;
    }
}