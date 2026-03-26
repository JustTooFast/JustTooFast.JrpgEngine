// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Runtime;

[TestClass]
public sealed class RandomDamageBattleActionDecoratorTests
{
    [TestMethod]
    public void Resolve_Should_Be_Deterministic_With_Seed()
    {
        var resolver1 = new RandomDamageBattleActionDecorator(
            new DefaultBattleActionResolver(), 1, 5, 0.0, 123);
        var resolver2 = new RandomDamageBattleActionDecorator(
            new DefaultBattleActionResolver(), 1, 5, 0.0, 123);

        var state = CreateState();

        var results1 = new[]
        {
            resolver1.Resolve(state, CreateAction()),
            resolver1.Resolve(state, CreateAction())
        };

        var results2 = new[]
        {
            resolver2.Resolve(state, CreateAction()),
            resolver2.Resolve(state, CreateAction())
        };

        Assert.AreEqual(results1[0].DamageDealt, results2[0].DamageDealt);
        Assert.AreEqual(results1[1].DamageDealt, results2[1].DamageDealt);
    }

    [TestMethod]
    public void Resolve_Should_Miss_When_MissChance_Is_One()
    {
        var resolver = new RandomDamageBattleActionDecorator(
            new DefaultBattleActionResolver(), 1, 5, 1.0, 123);

        var state = CreateState();

        var result = resolver.Resolve(state, CreateAction());

        Assert.IsTrue(result.WasMiss);
        Assert.AreEqual(0, result.DamageDealt);
    }

    [TestMethod]
    public void Constructor_Should_Throw_When_InnerResolver_Is_Null()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
            new RandomDamageBattleActionDecorator(
                null!,
                minDamage: 1,
                maxDamage: 5,
                missChance: 0.0,
                seed: 123));
    }

    [TestMethod]
    public void Constructor_Should_Throw_When_MinDamage_Is_Negative()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            new RandomDamageBattleActionDecorator(
                new DefaultBattleActionResolver(),
                minDamage: -1,
                maxDamage: 5,
                missChance: 0.0,
                seed: 123));
    }

    [TestMethod]
    public void Constructor_Should_Throw_When_MaxDamage_Is_Less_Than_MinDamage()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            new RandomDamageBattleActionDecorator(
                new DefaultBattleActionResolver(),
                minDamage: 5,
                maxDamage: 4,
                missChance: 0.0,
                seed: 123));
    }

    [TestMethod]
    public void Constructor_Should_Throw_When_MissChance_Is_Less_Than_Zero()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            new RandomDamageBattleActionDecorator(
                new DefaultBattleActionResolver(),
                minDamage: 1,
                maxDamage: 5,
                missChance: -0.01,
                seed: 123));
    }

    [TestMethod]
    public void Constructor_Should_Throw_When_MissChance_Is_Greater_Than_One()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            new RandomDamageBattleActionDecorator(
                new DefaultBattleActionResolver(),
                minDamage: 1,
                maxDamage: 5,
                missChance: 1.01,
                seed: 123));
    }

    [TestMethod]
    public void Resolve_Should_Delegate_NonAttack_Actions_To_Inner_Resolver()
    {
        IBattleActionResolver resolver = new RandomDamageBattleActionDecorator(
            new DefaultBattleActionResolver(),
            minDamage: 1,
            maxDamage: 5,
            missChance: 0.5,
            seed: 123);

        BattleActionResult result = resolver.Resolve(
            CreateState(),
            new BattleActionChoice("hero", BattleActionKind.Wait, targetId: null));

        Assert.AreEqual(BattleActionKind.Wait, result.ActionKind);
        Assert.AreEqual("hero", result.ActorId);
        Assert.IsNull(result.TargetId);
        Assert.AreEqual(0, result.DamageDealt);
        Assert.IsFalse(result.TargetDefeated);
        Assert.IsFalse(result.WasMiss);
        Assert.IsFalse(result.WasEscapeSuccessful);
    }

    [TestMethod]
    public void Resolve_Should_Not_Deal_More_Damage_Than_Target_CurrentHp()
    {
        IBattleActionResolver resolver = new RandomDamageBattleActionDecorator(
            new DefaultBattleActionResolver(),
            minDamage: 5,
            maxDamage: 5,
            missChance: 0.0,
            seed: 123);

        BattleState state = new(
        [
            new BattleCombatantState("hero", "Hero", BattleTeam.Party, 10, 10),
            new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 3, 10)
        ],
        false,
        BattleOutcome.None);

        BattleActionResult result = resolver.Resolve(state, CreateAction());

        Assert.AreEqual(3, result.DamageDealt);
        Assert.IsTrue(result.TargetDefeated);
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Target_Is_On_Same_Team()
    {
        IBattleActionResolver resolver = new RandomDamageBattleActionDecorator(
            new DefaultBattleActionResolver(),
            minDamage: 1,
            maxDamage: 5,
            missChance: 0.0,
            seed: 123);

        BattleState state = new(
        [
            new BattleCombatantState("hero_1", "Hero 1", BattleTeam.Party, 10, 10),
            new BattleCombatantState("hero_2", "Hero 2", BattleTeam.Party, 10, 10)
        ],
        false,
        BattleOutcome.None);

        BattleActionChoice action = new("hero_1", BattleActionKind.Attack, "hero_2");

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
            () => resolver.Resolve(state, action));

        Assert.AreEqual("Cannot target a combatant on the same team.", ex.Message);
    }

    private static BattleState CreateState()
    {
        return new BattleState(
        [
            new BattleCombatantState("hero", "Hero", BattleTeam.Party, 10, 10),
            new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 10, 10)
        ],
        false,
        BattleOutcome.None);
    }

    private static BattleActionChoice CreateAction()
    {
        return new BattleActionChoice("hero", BattleActionKind.Attack, "slime");
    }
}