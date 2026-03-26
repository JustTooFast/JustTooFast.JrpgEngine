// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

public sealed class BattleRuntimeFactory : IBattleRuntimeFactory
{
    private readonly Func<IBattleFlow> _flowFactory;
    private readonly Func<IEnemyActionChooser> _enemyActionChooserFactory;
    private readonly Func<IEnemyTargetChooser> _enemyTargetChooserFactory;
    private readonly Func<IBattleActionResolver> _actionResolverFactory;
    private readonly Func<IBattleRewardCalculator> _rewardCalculatorFactory;
    private readonly Func<IPlayerChoiceBlockingPolicy> _playerChoiceBlockingPolicyFactory;

    public BattleRuntimeFactory(
        Func<IBattleFlow> flowFactory,
        Func<IEnemyActionChooser> enemyActionChooserFactory,
        Func<IEnemyTargetChooser> enemyTargetChooserFactory,
        Func<IBattleActionResolver> actionResolverFactory,
        Func<IBattleRewardCalculator> rewardCalculatorFactory,
        Func<IPlayerChoiceBlockingPolicy> playerChoiceBlockingPolicyFactory)
    {
        _flowFactory = flowFactory ?? throw new ArgumentNullException(nameof(flowFactory));
        _enemyActionChooserFactory = enemyActionChooserFactory ?? throw new ArgumentNullException(nameof(enemyActionChooserFactory));
        _enemyTargetChooserFactory = enemyTargetChooserFactory ?? throw new ArgumentNullException(nameof(enemyTargetChooserFactory));
        _actionResolverFactory = actionResolverFactory ?? throw new ArgumentNullException(nameof(actionResolverFactory));
        _rewardCalculatorFactory = rewardCalculatorFactory ?? throw new ArgumentNullException(nameof(rewardCalculatorFactory));
        _playerChoiceBlockingPolicyFactory = playerChoiceBlockingPolicyFactory ?? throw new ArgumentNullException(nameof(playerChoiceBlockingPolicyFactory));
    }

    public IBattleRuntime Create(BattleDefinition definition)
    {
        if (definition is null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        return new BattleRuntime(
            definition,
            _flowFactory(),
            _enemyActionChooserFactory(),
            _enemyTargetChooserFactory(),
            _actionResolverFactory(),
            _rewardCalculatorFactory(),
            _playerChoiceBlockingPolicyFactory());
    }
}