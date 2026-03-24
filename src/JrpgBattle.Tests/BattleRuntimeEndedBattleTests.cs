// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests;

[TestClass]
public sealed class BattleRuntimeEndedBattleTests
{
    [TestMethod]
    public void ApplyAction_Should_Return_Same_State_When_Battle_Already_Ended()
    {
        // Arrange
        var runtime = new BattleRuntime(
            new FixedDamageBattleActionResolver(5),
            new FixedXpBattleRewardCalculator(10));

        var endedState = new BattleState(
            combatants:
            [
                new BattleCombatantState("hero", "Hero", BattleTeam.Party, 10, 10),
                new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 0, 10)
            ],
            isEnded: true,
            outcome: BattleOutcome.Victory);

        var action = new BattleActionChoice(
            actorId: "hero",
            actionKind: BattleActionKind.Attack,
            targetId: "slime");

        // Act
        var resultState = runtime.ApplyAction(endedState, action);

        // Assert
        Assert.AreSame(endedState, resultState);
    }

    [TestMethod]
    public void GetResult_Should_Throw_When_Battle_Not_Ended()
    {
        // Arrange
        var runtime = new BattleRuntime(
            new FixedDamageBattleActionResolver(5),
            new FixedXpBattleRewardCalculator(10));

        var definition = new BattleDefinition(
            partyCombatants:
            [
                new BattleCombatantDefinition("hero", "Hero", BattleTeam.Party, 20)
            ],
            enemyCombatants:
            [
                new BattleCombatantDefinition("slime", "Slime", BattleTeam.Enemy, 10)
            ]);

        var state = runtime.Initialize(definition);

        // Act + Assert
        var exception = Assert.ThrowsException<InvalidOperationException>(
            () => runtime.GetResult(state));

        Assert.AreEqual("Battle has not ended.", exception.Message);
    }

    [TestMethod]
    public void GetResult_Should_Return_Result_When_Battle_Ended()
    {
        // Arrange
        var runtime = new BattleRuntime(
            new FixedDamageBattleActionResolver(5),
            new FixedXpBattleRewardCalculator(10));

        var endedState = new BattleState(
            combatants:
            [
                new BattleCombatantState("hero", "Hero", BattleTeam.Party, 10, 10),
                new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 0, 10)
            ],
            isEnded: true,
            outcome: BattleOutcome.Victory);

        // Act
        var result = runtime.GetResult(endedState);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(BattleOutcome.Victory, result.Outcome);
        Assert.IsNotNull(result.Reward);
        Assert.AreEqual(10, result.Reward!.ExperiencePoints);
    }
}