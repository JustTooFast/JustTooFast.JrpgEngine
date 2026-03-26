// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Runtime;

[TestClass]
public sealed class FixedXpBattleRewardCalculatorTests
{
    [TestMethod]
    public void Constructor_Should_Throw_When_ExperiencePoints_Is_Negative()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new FixedXpBattleRewardCalculator(-1));
    }

    [TestMethod]
    public void Calculate_Should_Return_Configured_ExperiencePoints()
    {
        var calculator = new FixedXpBattleRewardCalculator(10);

        var finalState = new BattleState(
        [
            new BattleCombatantState("hero", "Hero", BattleTeam.Party, 10, 10),
            new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 0, 10)
        ],
        isEnded: true,
        outcome: BattleOutcome.Victory);

        BattleReward reward = calculator.Calculate(finalState);

        Assert.AreEqual(10, reward.ExperiencePoints);
    }

    [TestMethod]
    public void Calculate_Should_Throw_When_FinalState_Is_Null()
    {
        var calculator = new FixedXpBattleRewardCalculator(10);

        Assert.ThrowsException<ArgumentNullException>(() => calculator.Calculate(null!));
    }

    [TestMethod]
    public void Calculate_Should_Throw_When_Battle_Has_Not_Ended()
    {
        var calculator = new FixedXpBattleRewardCalculator(10);

        var state = new BattleState(
        [
            new BattleCombatantState("hero", "Hero", BattleTeam.Party, 10, 10),
            new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 10, 10)
        ],
        isEnded: false,
        outcome: BattleOutcome.None);

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
            () => calculator.Calculate(state));

        Assert.AreEqual("Cannot calculate rewards before the battle has ended.", ex.Message);
    }
}