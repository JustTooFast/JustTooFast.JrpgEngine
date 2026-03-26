// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Runtime;

[TestClass]
public sealed class EscapeChanceBattleActionDecoratorTests
{
    [TestMethod]
    public void Resolve_Should_Delegate_NonEscape_Actions_To_Inner_Resolver()
    {
        IBattleActionResolver resolver = new EscapeChanceBattleActionDecorator(
            new FixedDamageBattleActionDecorator(
                new DefaultBattleActionResolver(),
                damage: 5),
            escapeSuccessChance: 0.5,
            seed: 123);

        BattleState state = CreateState();

        BattleActionResult result = resolver.Resolve(
            state,
            new BattleActionChoice("hero", BattleActionKind.Attack, "slime"));

        Assert.AreEqual(BattleActionKind.Attack, result.ActionKind);
        Assert.AreEqual("hero", result.ActorId);
        Assert.AreEqual("slime", result.TargetId);
        Assert.AreEqual(5, result.DamageDealt);
        Assert.IsFalse(result.WasMiss);
        Assert.IsFalse(result.WasEscapeSuccessful);
    }

    [TestMethod]
    public void Resolve_Should_Always_Fail_Escape_When_Chance_Is_Zero()
    {
        IBattleActionResolver resolver = new EscapeChanceBattleActionDecorator(
            new DefaultBattleActionResolver(),
            escapeSuccessChance: 0.0,
            seed: 123);

        BattleState state = CreateState();

        BattleActionResult result = resolver.Resolve(
            state,
            new BattleActionChoice("hero", BattleActionKind.Escape, targetId: null));

        Assert.AreEqual(BattleActionKind.Escape, result.ActionKind);
        Assert.AreEqual("hero", result.ActorId);
        Assert.IsNull(result.TargetId);
        Assert.AreEqual(0, result.DamageDealt);
        Assert.IsFalse(result.TargetDefeated);
        Assert.IsFalse(result.WasMiss);
        Assert.IsFalse(result.WasEscapeSuccessful);
    }

    [TestMethod]
    public void Resolve_Should_Always_Succeed_Escape_When_Chance_Is_One()
    {
        IBattleActionResolver resolver = new EscapeChanceBattleActionDecorator(
            new DefaultBattleActionResolver(),
            escapeSuccessChance: 1.0,
            seed: 123);

        BattleState state = CreateState();

        BattleActionResult result = resolver.Resolve(
            state,
            new BattleActionChoice("hero", BattleActionKind.Escape, targetId: null));

        Assert.AreEqual(BattleActionKind.Escape, result.ActionKind);
        Assert.AreEqual("hero", result.ActorId);
        Assert.IsNull(result.TargetId);
        Assert.AreEqual(0, result.DamageDealt);
        Assert.IsFalse(result.TargetDefeated);
        Assert.IsFalse(result.WasMiss);
        Assert.IsTrue(result.WasEscapeSuccessful);
    }

    [TestMethod]
    public void Resolve_Should_Be_Deterministic_For_Escape_With_Same_Seed()
    {
        IBattleActionResolver resolver1 = new EscapeChanceBattleActionDecorator(
            new DefaultBattleActionResolver(),
            escapeSuccessChance: 0.5,
            seed: 123);

        IBattleActionResolver resolver2 = new EscapeChanceBattleActionDecorator(
            new DefaultBattleActionResolver(),
            escapeSuccessChance: 0.5,
            seed: 123);

        BattleState state = CreateState();

        bool[] results1 =
        [
            resolver1.Resolve(state, new BattleActionChoice("hero", BattleActionKind.Escape, null)).WasEscapeSuccessful,
            resolver1.Resolve(state, new BattleActionChoice("hero", BattleActionKind.Escape, null)).WasEscapeSuccessful,
            resolver1.Resolve(state, new BattleActionChoice("hero", BattleActionKind.Escape, null)).WasEscapeSuccessful
        ];

        bool[] results2 =
        [
            resolver2.Resolve(state, new BattleActionChoice("hero", BattleActionKind.Escape, null)).WasEscapeSuccessful,
            resolver2.Resolve(state, new BattleActionChoice("hero", BattleActionKind.Escape, null)).WasEscapeSuccessful,
            resolver2.Resolve(state, new BattleActionChoice("hero", BattleActionKind.Escape, null)).WasEscapeSuccessful
        ];

        CollectionAssert.AreEqual(results1, results2);
    }

    [TestMethod]
    public void Constructor_Should_Throw_When_InnerResolver_Is_Null()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
            new EscapeChanceBattleActionDecorator(
                null!,
                escapeSuccessChance: 0.5,
                seed: 123));
    }

    [TestMethod]
    public void Constructor_Should_Throw_When_EscapeSuccessChance_Is_Less_Than_Zero()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            new EscapeChanceBattleActionDecorator(
                new DefaultBattleActionResolver(),
                escapeSuccessChance: -0.01,
                seed: 123));
    }

    [TestMethod]
    public void Constructor_Should_Throw_When_EscapeSuccessChance_Is_Greater_Than_One()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            new EscapeChanceBattleActionDecorator(
                new DefaultBattleActionResolver(),
                escapeSuccessChance: 1.01,
                seed: 123));
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_State_Is_Null()
    {
        IBattleActionResolver resolver = new EscapeChanceBattleActionDecorator(
            new DefaultBattleActionResolver(),
            escapeSuccessChance: 0.5,
            seed: 123);

        Assert.ThrowsException<ArgumentNullException>(() =>
            resolver.Resolve(
                null!,
                new BattleActionChoice("hero", BattleActionKind.Escape, null)));
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Action_Is_Null()
    {
        IBattleActionResolver resolver = new EscapeChanceBattleActionDecorator(
            new DefaultBattleActionResolver(),
            escapeSuccessChance: 0.5,
            seed: 123);

        Assert.ThrowsException<ArgumentNullException>(() =>
            resolver.Resolve(CreateState(), null!));
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Escape_Actor_Is_Not_Found()
    {
        IBattleActionResolver resolver = new EscapeChanceBattleActionDecorator(
            new DefaultBattleActionResolver(),
            escapeSuccessChance: 0.5,
            seed: 123);

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(() =>
            resolver.Resolve(
                CreateState(),
                new BattleActionChoice("missing", BattleActionKind.Escape, null)));

        Assert.AreEqual("Actor 'missing' not found.", ex.Message);
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Escape_Actor_Is_Not_Alive()
    {
        IBattleActionResolver resolver = new EscapeChanceBattleActionDecorator(
            new DefaultBattleActionResolver(),
            escapeSuccessChance: 0.5,
            seed: 123);

        BattleState state = new(
        [
            new BattleCombatantState("hero", "Hero", BattleTeam.Party, 0, 10),
            new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 10, 10)
        ],
        false,
        BattleOutcome.None);

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(() =>
            resolver.Resolve(
                state,
                new BattleActionChoice("hero", BattleActionKind.Escape, null)));

        Assert.AreEqual("Actor is not alive.", ex.Message);
    }

    [TestMethod]
    public void Resolve_Should_Delegate_NonEscape_NoOp_Actions_To_Inner_Resolver()
    {
        IBattleActionResolver resolver = new EscapeChanceBattleActionDecorator(
            new DefaultBattleActionResolver(),
            escapeSuccessChance: 0.5,
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

    private static BattleState CreateState()
    {
        return new BattleState(
        [
            new BattleCombatantState("hero", "Hero", BattleTeam.Party, 10, 10),
            new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 10, 10)
        ],
        isEnded: false,
        outcome: BattleOutcome.None);
    }
}