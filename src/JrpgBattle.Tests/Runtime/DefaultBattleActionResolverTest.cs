// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Runtime;

[TestClass]
public sealed class DefaultBattleActionResolverTests
{
    [TestMethod]
    public void Resolve_Should_Throw_When_State_Is_Null()
    {
        var resolver = new DefaultBattleActionResolver();

        Assert.ThrowsException<ArgumentNullException>(() =>
            resolver.Resolve(
                null!,
                new BattleActionChoice("hero", BattleActionKind.Defend, targetId: null)));
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Action_Is_Null()
    {
        var resolver = new DefaultBattleActionResolver();

        Assert.ThrowsException<ArgumentNullException>(() =>
            resolver.Resolve(CreateDefaultState(), null!));
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Actor_Is_Not_Found()
    {
        var resolver = new DefaultBattleActionResolver();

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(() =>
            resolver.Resolve(
                CreateDefaultState(),
                new BattleActionChoice("missing", BattleActionKind.Defend, targetId: null)));

        Assert.AreEqual("Actor 'missing' not found.", ex.Message);
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Actor_Is_Not_Alive()
    {
        var resolver = new DefaultBattleActionResolver();

        BattleState state = new(
        [
            new BattleCombatantState("hero", "Hero", BattleTeam.Party, 0, 10),
            new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 10, 10)
        ],
        isEnded: false,
        outcome: BattleOutcome.None);

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(() =>
            resolver.Resolve(
                state,
                new BattleActionChoice("hero", BattleActionKind.Defend, targetId: null)));

        Assert.AreEqual("Actor is not alive.", ex.Message);
    }

    [TestMethod]
    public void Resolve_Should_Return_NoOp_Result_For_Attack()
    {
        var resolver = new DefaultBattleActionResolver();

        BattleActionResult result = resolver.Resolve(
            CreateDefaultState(),
            new BattleActionChoice("hero", BattleActionKind.Attack, "slime"));

        Assert.AreEqual("hero", result.ActorId);
        Assert.AreEqual("slime", result.TargetId);
        Assert.AreEqual(BattleActionKind.Attack, result.ActionKind);
        Assert.AreEqual(0, result.DamageDealt);
        Assert.IsFalse(result.TargetDefeated);
        Assert.IsFalse(result.WasMiss);
        Assert.IsFalse(result.WasEscapeSuccessful);
    }

    [TestMethod]
    public void Resolve_Should_Return_NoOp_Result_For_Magic()
    {
        var resolver = new DefaultBattleActionResolver();

        BattleActionResult result = resolver.Resolve(
            CreateDefaultState(),
            new BattleActionChoice("hero", BattleActionKind.Magic, "slime"));

        Assert.AreEqual("hero", result.ActorId);
        Assert.AreEqual("slime", result.TargetId);
        Assert.AreEqual(BattleActionKind.Magic, result.ActionKind);
        Assert.AreEqual(0, result.DamageDealt);
        Assert.IsFalse(result.TargetDefeated);
        Assert.IsFalse(result.WasMiss);
        Assert.IsFalse(result.WasEscapeSuccessful);
    }

    [TestMethod]
    public void Resolve_Should_Return_NoOp_Result_For_Item()
    {
        var resolver = new DefaultBattleActionResolver();

        BattleActionResult result = resolver.Resolve(
            CreateDefaultState(),
            new BattleActionChoice("hero", BattleActionKind.Item, "slime"));

        Assert.AreEqual("hero", result.ActorId);
        Assert.AreEqual("slime", result.TargetId);
        Assert.AreEqual(BattleActionKind.Item, result.ActionKind);
        Assert.AreEqual(0, result.DamageDealt);
        Assert.IsFalse(result.TargetDefeated);
        Assert.IsFalse(result.WasMiss);
        Assert.IsFalse(result.WasEscapeSuccessful);
    }

    [TestMethod]
    public void Resolve_Should_Return_NoOp_Result_For_Skill()
    {
        var resolver = new DefaultBattleActionResolver();

        BattleActionResult result = resolver.Resolve(
            CreateDefaultState(),
            new BattleActionChoice("hero", BattleActionKind.Skill, "slime"));

        Assert.AreEqual("hero", result.ActorId);
        Assert.AreEqual("slime", result.TargetId);
        Assert.AreEqual(BattleActionKind.Skill, result.ActionKind);
        Assert.AreEqual(0, result.DamageDealt);
        Assert.IsFalse(result.TargetDefeated);
        Assert.IsFalse(result.WasMiss);
        Assert.IsFalse(result.WasEscapeSuccessful);
    }

    [TestMethod]
    public void Resolve_Should_Allow_Null_Target_For_Magic()
    {
        var resolver = new DefaultBattleActionResolver();

        BattleActionResult result = resolver.Resolve(
            CreateDefaultState(),
            new BattleActionChoice("hero", BattleActionKind.Magic, targetId: null));

        Assert.AreEqual("hero", result.ActorId);
        Assert.IsNull(result.TargetId);
        Assert.AreEqual(BattleActionKind.Magic, result.ActionKind);
        Assert.AreEqual(0, result.DamageDealt);
        Assert.IsFalse(result.TargetDefeated);
        Assert.IsFalse(result.WasMiss);
        Assert.IsFalse(result.WasEscapeSuccessful);
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Target_Is_Not_Found_For_Targeted_Action()
    {
        var resolver = new DefaultBattleActionResolver();

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(() =>
            resolver.Resolve(
                CreateDefaultState(),
                new BattleActionChoice("hero", BattleActionKind.Attack, "missing")));

        Assert.AreEqual("Target 'missing' not found.", ex.Message);
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Target_Is_Not_Alive_For_Targeted_Action()
    {
        var resolver = new DefaultBattleActionResolver();

        BattleState state = new(
        [
            new BattleCombatantState("hero", "Hero", BattleTeam.Party, 10, 10),
            new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 0, 10)
        ],
        isEnded: false,
        outcome: BattleOutcome.None);

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(() =>
            resolver.Resolve(
                state,
                new BattleActionChoice("hero", BattleActionKind.Attack, "slime")));

        Assert.AreEqual("Target is not alive.", ex.Message);
    }

    [TestMethod]
    public void Resolve_Should_Return_NoOp_Result_For_Defend()
    {
        var resolver = new DefaultBattleActionResolver();

        BattleActionResult result = resolver.Resolve(
            CreateDefaultState(),
            new BattleActionChoice("hero", BattleActionKind.Defend, targetId: null));

        Assert.AreEqual("hero", result.ActorId);
        Assert.IsNull(result.TargetId);
        Assert.AreEqual(BattleActionKind.Defend, result.ActionKind);
        Assert.AreEqual(0, result.DamageDealt);
        Assert.IsFalse(result.TargetDefeated);
        Assert.IsFalse(result.WasMiss);
        Assert.IsFalse(result.WasEscapeSuccessful);
    }

    [TestMethod]
    public void Resolve_Should_Return_Successful_Result_For_Escape()
    {
        var resolver = new DefaultBattleActionResolver();

        BattleActionResult result = resolver.Resolve(
            CreateDefaultState(),
            new BattleActionChoice("hero", BattleActionKind.Escape, targetId: null));

        Assert.AreEqual("hero", result.ActorId);
        Assert.IsNull(result.TargetId);
        Assert.AreEqual(BattleActionKind.Escape, result.ActionKind);
        Assert.AreEqual(0, result.DamageDealt);
        Assert.IsFalse(result.TargetDefeated);
        Assert.IsFalse(result.WasMiss);
        Assert.IsTrue(result.WasEscapeSuccessful);
    }

    [TestMethod]
    public void Resolve_Should_Return_NoOp_Result_For_Wait()
    {
        var resolver = new DefaultBattleActionResolver();

        BattleActionResult result = resolver.Resolve(
            CreateDefaultState(),
            new BattleActionChoice("hero", BattleActionKind.Wait, targetId: null));

        Assert.AreEqual("hero", result.ActorId);
        Assert.IsNull(result.TargetId);
        Assert.AreEqual(BattleActionKind.Wait, result.ActionKind);
        Assert.AreEqual(0, result.DamageDealt);
        Assert.IsFalse(result.TargetDefeated);
        Assert.IsFalse(result.WasMiss);
        Assert.IsFalse(result.WasEscapeSuccessful);
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Action_Is_Not_Supported()
    {
        var resolver = new DefaultBattleActionResolver();
        var state = CreateDefaultState();

        BattleActionChoice action = new(
            actorId: "hero",
            actionKind: (BattleActionKind)999,
            targetId: null);

        NotSupportedException ex = Assert.ThrowsException<NotSupportedException>(
            () => resolver.Resolve(state, action));

        Assert.AreEqual("Action '999' is not supported.", ex.Message);
    }

    private static BattleState CreateDefaultState()
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