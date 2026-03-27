// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle.ConsoleHost;

public static class Program
{
    public static int Main(string[] args)
    {
        IBattleRuntimeFactory battleRuntimeFactory = new BattleRuntimeFactory(
            flowFactory: () => new TeamPhaseBattleFlow(BattleTeam.Party),
            enemyActionChooserFactory: () => new RandomEnemyActionChooser(Environment.TickCount),
            actionResolverFactory: () =>
                new EscapeChanceBattleActionDecorator(
                    new RandomDamageBattleActionDecorator(
                        new DefaultBattleActionResolver(),
                        minDamage: 3,
                        maxDamage: 7,
                        missChance: 0.10,
                        seed: Environment.TickCount),
                    escapeSuccessChance: 0.75,
                    seed: Environment.TickCount));

        IBattleRewardApplier battleRewardApplier = new ConsoleBattleRewardApplier();

        var app = new ConsoleBattleHostApp(
            battleRuntimeFactory,
            battleRewardApplier);

        return app.Run();
    }
}