// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgBattle.Abstractions.Contracts;

namespace JustTooFast.JrpgBattle;

public sealed class BattleRuntimeFactory : IBattleRuntimeFactory
{
    private readonly IBattleActionResolver _actionResolver;
    private readonly IBattleRewardCalculator _rewardCalculator;

    public BattleRuntimeFactory(
        IBattleActionResolver actionResolver,
        IBattleRewardCalculator rewardCalculator)
    {
        _actionResolver = actionResolver ?? throw new ArgumentNullException(nameof(actionResolver));
        _rewardCalculator = rewardCalculator ?? throw new ArgumentNullException(nameof(rewardCalculator));
    }

    public IBattleRuntime Create()
    {
        return new BattleRuntime(
            _actionResolver,
            _rewardCalculator);
    }
}