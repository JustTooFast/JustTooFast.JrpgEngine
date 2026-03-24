// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

public sealed class BattleRuntimeFactory : IBattleRuntimeFactory
{
    private readonly IBattleFlow _flow;
    private readonly IEnemyActionChooser _enemyActionChooser;
    private readonly IEnemyTargetChooser _enemyTargetChooser;
    private readonly IBattleActionResolver _actionResolver;
    private readonly IBattleRewardCalculator _rewardCalculator;
    private readonly IPlayerChoiceBlockingPolicy _playerChoiceBlockingPolicy;

    public BattleRuntimeFactory(
        IBattleFlow flow,
        IEnemyActionChooser enemyActionChooser,
        IEnemyTargetChooser enemyTargetChooser,
        IBattleActionResolver actionResolver,
        IBattleRewardCalculator rewardCalculator,
        IPlayerChoiceBlockingPolicy playerChoiceBlockingPolicy)
    {
        _flow = flow ?? throw new ArgumentNullException(nameof(flow));
        _enemyActionChooser = enemyActionChooser ?? throw new ArgumentNullException(nameof(enemyActionChooser));
        _enemyTargetChooser = enemyTargetChooser ?? throw new ArgumentNullException(nameof(enemyTargetChooser));
        _actionResolver = actionResolver ?? throw new ArgumentNullException(nameof(actionResolver));
        _rewardCalculator = rewardCalculator ?? throw new ArgumentNullException(nameof(rewardCalculator));
        _playerChoiceBlockingPolicy = playerChoiceBlockingPolicy ?? throw new ArgumentNullException(nameof(playerChoiceBlockingPolicy));
    }

    public IBattleRuntime Create(BattleDefinition definition)
    {
        if (definition is null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        return new BattleRuntime(
            definition,
            _flow,
            _enemyActionChooser,
            _enemyTargetChooser,
            _actionResolver,
            _rewardCalculator,
            _playerChoiceBlockingPolicy);
    }
}