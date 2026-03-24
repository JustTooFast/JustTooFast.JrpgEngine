// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using JustTooFast.JrpgBattle;

namespace JustTooFast.JrpgBattle.ConsoleHost;

public static class Program
{
    public static int Main(string[] args)
    {
        var battleRuntimeFactory = new BattleRuntimeFactory(
            new FixedDamageBattleActionResolver(),
            new FixedXpBattleRewardCalculator(experiencePoints: 10));

        var app = new ConsoleBattleHostApp(battleRuntimeFactory);

        return app.Run();
    }
}