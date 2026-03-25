// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Runtime;

[TestClass]
public sealed class BattleRuntimeNonBlockingPolicyTests
{
    [TestMethod]
    public void Advance_Should_Continue_Advancing_When_PlayerChoice_Is_Pending_And_BlockingPolicy_Does_Not_Block()
    {
        // Arrange
        BattleDefinition definition = new(
            [new BattleCombatantDefinition("hero", "Hero", BattleTeam.Party, 20)],
            [new BattleCombatantDefinition("slime_1", "Slime 1", BattleTeam.Enemy, 10)]);

        IBattleRuntimeFactory factory = new BattleRuntimeFactory(
            new FirstLivingBattleFlow(),
            new AlwaysAttackEnemyActionChooser(),
            new FirstLivingEnemyTargetChooser(),
            new FixedDamageBattleActionResolver(5),
            new FixedXpBattleRewardCalculator(10),
            new NeverBlockOnPlayerChoicePolicy());

        IBattleRuntime runtime = factory.Create(definition);

        // Act
        BattleAdvanceResult first = runtime.Advance();
        BattleAdvanceResult second = runtime.Advance();

        // Assert
        Assert.IsTrue(first.IsPlayerInputNeeded);
        Assert.IsTrue(second.IsPlayerInputNeeded);
    }

    private sealed class NeverBlockOnPlayerChoicePolicy : IPlayerChoiceBlockingPolicy
    {
        public bool ShouldBlockAdvance(BattleRuntimeView runtimeView)
        {
            return false;
        }
    }
}