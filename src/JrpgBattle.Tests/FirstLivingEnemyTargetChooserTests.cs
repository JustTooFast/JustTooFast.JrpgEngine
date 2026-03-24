// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Ai;

[TestClass]
public sealed class FirstLivingEnemyTargetChooserTests
{
    [TestMethod]
    public void ChooseTargetId_Should_Return_First_Living_Opponent()
    {
        // Arrange
        var chooser = new FirstLivingEnemyTargetChooser();

        var state = new BattleState(
            combatants:
            [
                new BattleCombatantState("hero_1", "Hero 1", BattleTeam.Party, 0, 10),
                new BattleCombatantState("hero_2", "Hero 2", BattleTeam.Party, 10, 10),
                new BattleCombatantState("slime_1", "Slime 1", BattleTeam.Enemy, 10, 10)
            ],
            isEnded: false,
            outcome: BattleOutcome.None);

        // Act
        var targetId = chooser.ChooseTargetId(state, "slime_1");

        // Assert
        Assert.AreEqual("hero_2", targetId);
    }

    [TestMethod]
    public void ChooseTargetId_Should_Work_For_Party_Actor()
    {
        // Arrange
        var chooser = new FirstLivingEnemyTargetChooser();

        var state = new BattleState(
            combatants:
            [
                new BattleCombatantState("hero_1", "Hero 1", BattleTeam.Party, 10, 10),
                new BattleCombatantState("slime_1", "Slime 1", BattleTeam.Enemy, 0, 10),
                new BattleCombatantState("slime_2", "Slime 2", BattleTeam.Enemy, 10, 10)
            ],
            isEnded: false,
            outcome: BattleOutcome.None);

        // Act
        var targetId = chooser.ChooseTargetId(state, "hero_1");

        // Assert
        Assert.AreEqual("slime_2", targetId);
    }

    [TestMethod]
    public void ChooseTargetId_Should_Throw_When_Actor_Not_Found()
    {
        // Arrange
        var chooser = new FirstLivingEnemyTargetChooser();

        var state = new BattleState(
            combatants:
            [
                new BattleCombatantState("hero_1", "Hero 1", BattleTeam.Party, 10, 10)
            ],
            isEnded: false,
            outcome: BattleOutcome.None);

        // Act + Assert
        var exception = Assert.ThrowsException<InvalidOperationException>(
            () => chooser.ChooseTargetId(state, "missing"));

        Assert.AreEqual("Actor 'missing' not found.", exception.Message);
    }

    [TestMethod]
    public void ChooseTargetId_Should_Throw_When_Actor_Is_Not_Alive()
    {
        // Arrange
        var chooser = new FirstLivingEnemyTargetChooser();

        var state = new BattleState(
            combatants:
            [
                new BattleCombatantState("hero_1", "Hero 1", BattleTeam.Party, 0, 10),
                new BattleCombatantState("slime_1", "Slime 1", BattleTeam.Enemy, 10, 10)
            ],
            isEnded: false,
            outcome: BattleOutcome.None);

        // Act + Assert
        var exception = Assert.ThrowsException<InvalidOperationException>(
            () => chooser.ChooseTargetId(state, "hero_1"));

        Assert.AreEqual("Actor is not alive.", exception.Message);
    }

    [TestMethod]
    public void ChooseTargetId_Should_Throw_When_No_Valid_Targets()
    {
        // Arrange
        var chooser = new FirstLivingEnemyTargetChooser();

        var state = new BattleState(
            combatants:
            [
                new BattleCombatantState("hero_1", "Hero 1", BattleTeam.Party, 10, 10),
                new BattleCombatantState("slime_1", "Slime 1", BattleTeam.Enemy, 0, 10)
            ],
            isEnded: false,
            outcome: BattleOutcome.None);

        // Act + Assert
        var exception = Assert.ThrowsException<InvalidOperationException>(
            () => chooser.ChooseTargetId(state, "hero_1"));

        Assert.AreEqual("No valid target found.", exception.Message);
    }
}