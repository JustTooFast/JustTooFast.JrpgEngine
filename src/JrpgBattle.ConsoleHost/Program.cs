// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using JustTooFast.JrpgBattle;

namespace JustTooFast.JrpgBattle.ConsoleHost;

public static class Program
{
    public static int Main(string[] args)
    {
        var battleRuntimeFactory = new BattleRuntimeFactory(
            new FixedDamageBattleActionResolver(damage: 5),
            new FixedXpBattleRewardCalculator(experiencePoints: 10));

        var app = new ConsoleBattleHostApp(
            battleRuntimeFactory,
            new FirstLivingBattleFlow(),
            new AlwaysAttackEnemyActionChooser(),
            new FirstLivingEnemyTargetChooser(),
            new ConsoleBattleRewardApplier());

        return app.Run();
    }
}