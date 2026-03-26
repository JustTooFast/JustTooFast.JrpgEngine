// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Contracts;

namespace JustTooFast.JrpgBattle.ConsoleHost;

public static class Program
{
    public static int Main(string[] args)
    {
        IBattleActionResolver actionResolver =
            new EscapeChanceBattleActionDecorator(
                new FixedDamageBattleActionDecorator(
                    new DefaultBattleActionResolver(),
                    damage: 5),
                escapeSuccessChance: 1.0,
                seed: 12345);

        IBattleRuntimeFactory battleRuntimeFactory = new BattleRuntimeFactory(
            new FirstLivingBattleFlow(),
            new AlwaysAttackEnemyActionChooser(),
            new FirstLivingEnemyTargetChooser(),
            actionResolver,
            new FixedXpBattleRewardCalculator(experiencePoints: 10),
            new AlwaysBlockOnPlayerChoicePolicy());

        IBattleRewardApplier battleRewardApplier = new ConsoleBattleRewardApplier();

        var app = new ConsoleBattleHostApp(
            battleRuntimeFactory,
            battleRewardApplier);

        return app.Run();
    }
}