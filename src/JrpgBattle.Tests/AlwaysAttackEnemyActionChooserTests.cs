// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Ai;

[TestClass]
public sealed class AlwaysAttackEnemyActionChooserTests
{
    [TestMethod]
    public void ChooseAction_Should_Return_Attack()
    {
        // Arrange
        var chooser = new AlwaysAttackEnemyActionChooser();

        var state = new BattleState(
            combatants:
            [
                new BattleCombatantState("slime_1", "Slime 1", BattleTeam.Enemy, 10, 10),
                new BattleCombatantState("hero_1", "Hero 1", BattleTeam.Party, 10, 10)
            ],
            isEnded: false,
            outcome: BattleOutcome.None);

        // Act
        BattleActionKind action = chooser.ChooseAction(state, "slime_1");

        // Assert
        Assert.AreEqual(BattleActionKind.Attack, action);
    }

    [TestMethod]
    public void ChooseAction_Should_Throw_When_State_Is_Null()
    {
        // Arrange
        var chooser = new AlwaysAttackEnemyActionChooser();

        // Act + Assert
        Assert.ThrowsException<ArgumentNullException>(
            () => chooser.ChooseAction(null!, "slime_1"));
    }

    [TestMethod]
    public void ChooseAction_Should_Throw_When_ActorId_Is_Missing()
    {
        // Arrange
        var chooser = new AlwaysAttackEnemyActionChooser();

        var state = new BattleState(
            combatants:
            [
                new BattleCombatantState("slime_1", "Slime 1", BattleTeam.Enemy, 10, 10),
                new BattleCombatantState("hero_1", "Hero 1", BattleTeam.Party, 10, 10)
            ],
            isEnded: false,
            outcome: BattleOutcome.None);

        // Act + Assert
        Assert.ThrowsException<ArgumentException>(
            () => chooser.ChooseAction(state, ""));
    }
}