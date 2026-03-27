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
    private readonly Func<IBattleActionResolver> _actionResolverFactory;

    public BattleRuntimeFactory(
        Func<IBattleFlow> flowFactory,
        Func<IEnemyActionChooser> enemyActionChooserFactory,
        Func<IBattleActionResolver> actionResolverFactory)
    {
        _flowFactory = flowFactory ?? throw new ArgumentNullException(nameof(flowFactory));
        _enemyActionChooserFactory = enemyActionChooserFactory ?? throw new ArgumentNullException(nameof(enemyActionChooserFactory));
        _actionResolverFactory = actionResolverFactory ?? throw new ArgumentNullException(nameof(actionResolverFactory));
    }

    public IBattleRuntime Create(BattleDefinition definition)
    {
        if (definition is null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        IBattleFlow flow = _flowFactory()
            ?? throw new InvalidOperationException("Flow factory returned null.");

        IEnemyActionChooser enemyActionChooser = _enemyActionChooserFactory()
            ?? throw new InvalidOperationException("Enemy action chooser factory returned null.");

        IBattleActionResolver actionResolver = _actionResolverFactory()
            ?? throw new InvalidOperationException("Action resolver factory returned null.");

        return new BattleRuntime(
            definition,
            flow,
            enemyActionChooser,
            actionResolver);
    }
}