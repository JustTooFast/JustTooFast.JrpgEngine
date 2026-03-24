// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Flow;

[TestClass]
public sealed class FirstLivingBattleFlowTests
{
    [TestMethod]
    public void GetNextActorId_Should_Return_First_Living_Combatant()
    {
        // Arrange
        var flow = new FirstLivingBattleFlow();

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
        string actorId = flow.GetNextActorId(state);

        // Assert
        Assert.AreEqual("hero_2", actorId);
    }

    [TestMethod]
    public void GetNextActorId_Should_Throw_When_State_Is_Null()
    {
        // Arrange
        var flow = new FirstLivingBattleFlow();

        // Act + Assert
        Assert.ThrowsException<ArgumentNullException>(() => flow.GetNextActorId(null!));
    }

    [TestMethod]
    public void GetNextActorId_Should_Throw_When_No_Living_Combatants_Exist()
    {
        // Arrange
        var flow = new FirstLivingBattleFlow();

        var state = new BattleState(
            combatants:
            [
                new BattleCombatantState("hero_1", "Hero 1", BattleTeam.Party, 0, 10),
                new BattleCombatantState("slime_1", "Slime 1", BattleTeam.Enemy, 0, 10)
            ],
            isEnded: false,
            outcome: BattleOutcome.None);

        // Act + Assert
        var exception = Assert.ThrowsException<InvalidOperationException>(
            () => flow.GetNextActorId(state));

        Assert.AreEqual("No living combatants found.", exception.Message);
    }
}