// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Runtime;

[TestClass]
public sealed class FixedDamageBattleActionResolverTests
{
    [TestMethod]
    public void Resolve_Should_Deal_Fixed_Damage()
    {
        // Arrange
        var resolver = new FixedDamageBattleActionResolver(5);

        var state = CreateState(
            new("hero", "Hero", BattleTeam.Party, 10, 10),
            new("slime", "Slime", BattleTeam.Enemy, 10, 10));

        var action = new BattleActionChoice("hero", BattleActionKind.Attack, "slime");

        // Act
        var result = resolver.Resolve(state, action);

        // Assert
        Assert.AreEqual(5, result.DamageDealt);
        Assert.IsFalse(result.TargetDefeated);
    }

    [TestMethod]
    public void Resolve_Should_Not_Deal_More_Damage_Than_Target_Hp()
    {
        // Arrange
        var resolver = new FixedDamageBattleActionResolver(10);

        var state = CreateState(
            new("hero", "Hero", BattleTeam.Party, 10, 10),
            new("slime", "Slime", BattleTeam.Enemy, 3, 10));

        var action = new BattleActionChoice("hero", BattleActionKind.Attack, "slime");

        // Act
        var result = resolver.Resolve(state, action);

        // Assert
        Assert.AreEqual(3, result.DamageDealt);
        Assert.IsTrue(result.TargetDefeated);
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Actor_Not_Found()
    {
        var resolver = new FixedDamageBattleActionResolver(5);
        var state = CreateDefaultState();

        var action = new BattleActionChoice("missing", BattleActionKind.Attack, "slime");

        var ex = Assert.ThrowsException<InvalidOperationException>(
            () => resolver.Resolve(state, action));

        Assert.AreEqual("Actor 'missing' not found.", ex.Message);
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Target_Not_Found()
    {
        var resolver = new FixedDamageBattleActionResolver(5);
        var state = CreateDefaultState();

        var action = new BattleActionChoice("hero", BattleActionKind.Attack, "missing");

        var ex = Assert.ThrowsException<InvalidOperationException>(
            () => resolver.Resolve(state, action));

        Assert.AreEqual("Target 'missing' not found.", ex.Message);
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Target_Is_Same_Team()
    {
        var resolver = new FixedDamageBattleActionResolver(5);

        var state = CreateState(
            new("hero_1", "Hero 1", BattleTeam.Party, 10, 10),
            new("hero_2", "Hero 2", BattleTeam.Party, 10, 10));

        var action = new BattleActionChoice("hero_1", BattleActionKind.Attack, "hero_2");

        var ex = Assert.ThrowsException<InvalidOperationException>(
            () => resolver.Resolve(state, action));

        Assert.AreEqual("Cannot target a combatant on the same team.", ex.Message);
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Actor_Not_Alive()
    {
        var resolver = new FixedDamageBattleActionResolver(5);

        var state = CreateState(
            new("hero", "Hero", BattleTeam.Party, 0, 10),
            new("slime", "Slime", BattleTeam.Enemy, 10, 10));

        var action = new BattleActionChoice("hero", BattleActionKind.Attack, "slime");

        var ex = Assert.ThrowsException<InvalidOperationException>(
            () => resolver.Resolve(state, action));

        Assert.AreEqual("Actor is not alive.", ex.Message);
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Target_Not_Alive()
    {
        var resolver = new FixedDamageBattleActionResolver(5);

        var state = CreateState(
            new("hero", "Hero", BattleTeam.Party, 10, 10),
            new("slime", "Slime", BattleTeam.Enemy, 0, 10));

        var action = new BattleActionChoice("hero", BattleActionKind.Attack, "slime");

        var ex = Assert.ThrowsException<InvalidOperationException>(
            () => resolver.Resolve(state, action));

        Assert.AreEqual("Target is not alive.", ex.Message);
    }

    private static BattleState CreateDefaultState()
    {
        return CreateState(
            new("hero", "Hero", BattleTeam.Party, 10, 10),
            new("slime", "Slime", BattleTeam.Enemy, 10, 10));
    }

    private static BattleState CreateState(params BattleCombatantState[] combatants)
    {
        return new BattleState(
            combatants,
            isEnded: false,
            outcome: BattleOutcome.None);
    }
}