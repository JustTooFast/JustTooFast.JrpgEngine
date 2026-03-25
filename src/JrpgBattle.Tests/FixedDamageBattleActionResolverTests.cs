// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Runtime;

[TestClass]
public sealed class FixedDamageBattleActionResolverTests
{
    [TestMethod]
    public void Constructor_Should_Throw_When_Damage_Is_Not_Positive()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new FixedDamageBattleActionResolver(0));
    }

    [TestMethod]
    public void Resolve_Should_Deal_Configured_Damage_For_Attack()
    {
        var resolver = new FixedDamageBattleActionResolver(5);
        var state = CreateState(
            new BattleCombatantState("hero", "Hero", BattleTeam.Party, 10, 10),
            new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 10, 10));
        var action = new BattleActionChoice("hero", BattleActionKind.Attack, "slime");

        BattleActionResult result = resolver.Resolve(state, action);

        Assert.AreEqual(5, result.DamageDealt);
        Assert.IsFalse(result.TargetDefeated);
    }

    [TestMethod]
    public void Resolve_Should_Return_ZeroDamage_For_Defend()
    {
        var resolver = new FixedDamageBattleActionResolver(5);
        var state = CreateState(
            new BattleCombatantState("hero", "Hero", BattleTeam.Party, 10, 10),
            new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 10, 10));
        var action = new BattleActionChoice("hero", BattleActionKind.Defend, targetId: null);

        BattleActionResult result = resolver.Resolve(state, action);

        Assert.AreEqual(0, result.DamageDealt);
        Assert.IsFalse(result.TargetDefeated);
        Assert.IsNull(result.TargetId);
    }

    [TestMethod]
    public void Resolve_Should_Return_ZeroDamage_For_Escape()
    {
        var resolver = new FixedDamageBattleActionResolver(5);
        var state = CreateState(
            new BattleCombatantState("hero", "Hero", BattleTeam.Party, 10, 10),
            new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 10, 10));
        var action = new BattleActionChoice("hero", BattleActionKind.Escape, targetId: null);

        BattleActionResult result = resolver.Resolve(state, action);

        Assert.AreEqual(0, result.DamageDealt);
        Assert.IsFalse(result.TargetDefeated);
        Assert.IsNull(result.TargetId);
    }

    [TestMethod]
    public void Resolve_Should_Not_Deal_More_Damage_Than_Target_CurrentHp()
    {
        var resolver = new FixedDamageBattleActionResolver(10);
        var state = CreateState(
            new BattleCombatantState("hero", "Hero", BattleTeam.Party, 10, 10),
            new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 3, 10));
        var action = new BattleActionChoice("hero", BattleActionKind.Attack, "slime");

        BattleActionResult result = resolver.Resolve(state, action);

        Assert.AreEqual(3, result.DamageDealt);
    }

    [TestMethod]
    public void Resolve_Should_Set_TargetDefeated_When_Damage_Reduces_Target_To_Zero()
    {
        var resolver = new FixedDamageBattleActionResolver(10);
        var state = CreateState(
            new BattleCombatantState("hero", "Hero", BattleTeam.Party, 10, 10),
            new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 3, 10));
        var action = new BattleActionChoice("hero", BattleActionKind.Attack, "slime");

        BattleActionResult result = resolver.Resolve(state, action);

        Assert.IsTrue(result.TargetDefeated);
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_State_Is_Null()
    {
        var resolver = new FixedDamageBattleActionResolver(5);
        var action = new BattleActionChoice("hero", BattleActionKind.Attack, "slime");

        Assert.ThrowsException<ArgumentNullException>(() => resolver.Resolve(null!, action));
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Action_Is_Null()
    {
        var resolver = new FixedDamageBattleActionResolver(5);
        var state = CreateState(
            new BattleCombatantState("hero", "Hero", BattleTeam.Party, 10, 10),
            new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 10, 10));

        Assert.ThrowsException<ArgumentNullException>(() => resolver.Resolve(state, null!));
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Actor_Not_Found()
    {
        var resolver = new FixedDamageBattleActionResolver(5);
        var state = CreateDefaultState();
        var action = new BattleActionChoice("missing", BattleActionKind.Attack, "slime");

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
            () => resolver.Resolve(state, action));

        Assert.AreEqual("Actor 'missing' not found.", ex.Message);
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Target_Not_Found()
    {
        var resolver = new FixedDamageBattleActionResolver(5);
        var state = CreateDefaultState();
        var action = new BattleActionChoice("hero", BattleActionKind.Attack, "missing");

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
            () => resolver.Resolve(state, action));

        Assert.AreEqual("Target 'missing' not found.", ex.Message);
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Actor_Is_Not_Alive()
    {
        var resolver = new FixedDamageBattleActionResolver(5);
        var state = CreateState(
            new BattleCombatantState("hero", "Hero", BattleTeam.Party, 0, 10),
            new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 10, 10));
        var action = new BattleActionChoice("hero", BattleActionKind.Attack, "slime");

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
            () => resolver.Resolve(state, action));

        Assert.AreEqual("Actor is not alive.", ex.Message);
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Target_Is_Not_Alive()
    {
        var resolver = new FixedDamageBattleActionResolver(5);
        var state = CreateState(
            new BattleCombatantState("hero", "Hero", BattleTeam.Party, 10, 10),
            new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 0, 10));
        var action = new BattleActionChoice("hero", BattleActionKind.Attack, "slime");

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
            () => resolver.Resolve(state, action));

        Assert.AreEqual("Target is not alive.", ex.Message);
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Target_Is_On_Same_Team()
    {
        var resolver = new FixedDamageBattleActionResolver(5);
        var state = CreateState(
            new BattleCombatantState("hero_1", "Hero 1", BattleTeam.Party, 10, 10),
            new BattleCombatantState("hero_2", "Hero 2", BattleTeam.Party, 10, 10));
        var action = new BattleActionChoice("hero_1", BattleActionKind.Attack, "hero_2");

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
            () => resolver.Resolve(state, action));

        Assert.AreEqual("Cannot target a combatant on the same team.", ex.Message);
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Action_Is_Not_Supported()
    {
        var resolver = new FixedDamageBattleActionResolver(5);
        var state = CreateDefaultState();

        BattleActionChoice action = new(
            actorId: "hero",
            actionKind: (BattleActionKind)999,
            targetId: "slime");

        NotSupportedException ex = Assert.ThrowsException<NotSupportedException>(
            () => resolver.Resolve(state, action));

        Assert.AreEqual("Action '999' is not supported in v0.", ex.Message);
    }

    private static BattleState CreateDefaultState()
    {
        return CreateState(
            new BattleCombatantState("hero", "Hero", BattleTeam.Party, 10, 10),
            new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 10, 10));
    }

    private static BattleState CreateState(params BattleCombatantState[] combatants)
    {
        return new BattleState(
            combatants,
            isEnded: false,
            outcome: BattleOutcome.None);
    }
}