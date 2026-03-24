// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests;

[TestClass]
public sealed class BattleRuntimeInvalidActionTests
{
    [TestMethod]
    public void ApplyAction_Should_Throw_When_Actor_Does_Not_Exist()
    {
        // Arrange
        var runtime = new BattleRuntime(
            new FixedDamageBattleActionResolver(5),
            new FixedXpBattleRewardCalculator(10));

        var state = runtime.Initialize(CreateBattleDefinition());

        var action = new BattleActionChoice(
            actorId: "missing_actor",
            actionKind: BattleActionKind.Attack,
            targetId: "slime");

        // Act + Assert
        var exception = Assert.ThrowsException<InvalidOperationException>(
            () => runtime.ApplyAction(state, action));

        Assert.AreEqual("Actor 'missing_actor' not found.", exception.Message);
    }

    [TestMethod]
    public void ApplyAction_Should_Throw_When_Target_Does_Not_Exist()
    {
        // Arrange
        var runtime = new BattleRuntime(
            new FixedDamageBattleActionResolver(5),
            new FixedXpBattleRewardCalculator(10));

        var state = runtime.Initialize(CreateBattleDefinition());

        var action = new BattleActionChoice(
            actorId: "hero",
            actionKind: BattleActionKind.Attack,
            targetId: "missing_target");

        // Act + Assert
        var exception = Assert.ThrowsException<InvalidOperationException>(
            () => runtime.ApplyAction(state, action));

        Assert.AreEqual("Target 'missing_target' not found.", exception.Message);
    }

    [TestMethod]
    public void ApplyAction_Should_Throw_When_Target_Is_On_Same_Team()
    {
        // Arrange
        var runtime = new BattleRuntime(
            new FixedDamageBattleActionResolver(5),
            new FixedXpBattleRewardCalculator(10));

        var definition = new BattleDefinition(
            partyCombatants:
            [
                new BattleCombatantDefinition("hero_1", "Hero 1", BattleTeam.Party, maxHp: 20),
                new BattleCombatantDefinition("hero_2", "Hero 2", BattleTeam.Party, maxHp: 20)
            ],
            enemyCombatants:
            [
                new BattleCombatantDefinition("slime", "Slime", BattleTeam.Enemy, maxHp: 10)
            ]);

        var state = runtime.Initialize(definition);

        var action = new BattleActionChoice(
            actorId: "hero_1",
            actionKind: BattleActionKind.Attack,
            targetId: "hero_2");

        // Act + Assert
        var exception = Assert.ThrowsException<InvalidOperationException>(
            () => runtime.ApplyAction(state, action));

        Assert.AreEqual("Cannot target a combatant on the same team.", exception.Message);
    }

    private static BattleDefinition CreateBattleDefinition()
    {
        return new BattleDefinition(
            partyCombatants:
            [
                new BattleCombatantDefinition("hero", "Hero", BattleTeam.Party, maxHp: 20)
            ],
            enemyCombatants:
            [
                new BattleCombatantDefinition("slime", "Slime", BattleTeam.Enemy, maxHp: 10)
            ]);
    }
}