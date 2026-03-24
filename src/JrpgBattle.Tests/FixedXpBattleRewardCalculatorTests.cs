// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Runtime;

[TestClass]
public sealed class FixedXpBattleRewardCalculatorTests
{
    [TestMethod]
    public void Calculate_Should_Return_Configured_ExperiencePoints()
    {
        // Arrange
        var calculator = new FixedXpBattleRewardCalculator(10);

        var finalState = new BattleState(
            combatants:
            [
                new BattleCombatantState("hero", "Hero", BattleTeam.Party, 10, 10),
                new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 0, 10)
            ],
            isEnded: true,
            outcome: BattleOutcome.Victory);

        // Act
        var reward = calculator.Calculate(finalState);

        // Assert
        Assert.IsNotNull(reward);
        Assert.AreEqual(10, reward.ExperiencePoints);
    }

    [TestMethod]
    public void Calculate_Should_Throw_When_FinalState_Is_Null()
    {
        // Arrange
        var calculator = new FixedXpBattleRewardCalculator(10);

        // Act + Assert
        Assert.ThrowsException<ArgumentNullException>(
            () => calculator.Calculate(null!));
    }

    [TestMethod]
    public void Calculate_Should_Throw_When_Battle_Has_Not_Ended()
    {
        // Arrange
        var calculator = new FixedXpBattleRewardCalculator(10);

        var state = new BattleState(
            combatants:
            [
                new BattleCombatantState("hero", "Hero", BattleTeam.Party, 10, 10),
                new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 10, 10)
            ],
            isEnded: false,
            outcome: BattleOutcome.None);

        // Act + Assert
        var ex = Assert.ThrowsException<InvalidOperationException>(
            () => calculator.Calculate(state));

        Assert.AreEqual("Cannot calculate rewards before the battle has ended.", ex.Message);
    }
}