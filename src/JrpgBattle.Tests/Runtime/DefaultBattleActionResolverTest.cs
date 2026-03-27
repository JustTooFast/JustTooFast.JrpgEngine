// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
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
            resolver.Resolve(null!, "hero", new BattleActionChoice(BattleActionKind.Defend, null, null)));
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Action_Is_Null()
    {
        var resolver = new DefaultBattleActionResolver();

        Assert.ThrowsException<ArgumentNullException>(() =>
            resolver.Resolve(CreateDefaultState(), "hero", null!));
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Actor_Is_Not_Found()
    {
        var resolver = new DefaultBattleActionResolver();

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(() =>
            resolver.Resolve(
                CreateDefaultState(),
                "missing",
                new BattleActionChoice(BattleActionKind.Defend, null, null)));

        Assert.AreEqual("Actor 'missing' not found.", ex.Message);
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Actor_Is_Defeated()
    {
        var resolver = new DefaultBattleActionResolver();

        BattleState state = new(
        [
            new BattleActorState("hero", "Hero", BattleTeam.Party, 0, 10),
            new BattleActorState("slime", "Slime", BattleTeam.Enemy, 10, 10)
        ]);

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(() =>
            resolver.Resolve(
                state,
                "hero",
                new BattleActionChoice(BattleActionKind.Defend, null, null)));

        Assert.AreEqual("Actor is defeated.", ex.Message);
    }

    [TestMethod]
    public void Resolve_Should_Return_Damage_Operation_For_Valid_Attack_Target()
    {
        var resolver = new DefaultBattleActionResolver();

        BattleResolution resolution = resolver.Resolve(
            CreateDefaultState(),
            "hero",
            new BattleActionChoice(BattleActionKind.Attack, null, new[] { "slime" }));

        Assert.AreEqual(1, resolution.Operations.Count);
        Assert.IsInstanceOfType<DamageOperation>(resolution.Operations.Single());

        var damage = (DamageOperation)resolution.Operations.Single();
        Assert.AreEqual("slime", damage.TargetId);
        Assert.AreEqual(1, damage.Amount);
    }

    [TestMethod]
    public void Resolve_Should_Return_Empty_Resolution_For_Attack_With_No_Targets()
    {
        var resolver = new DefaultBattleActionResolver();

        BattleResolution resolution = resolver.Resolve(
            CreateDefaultState(),
            "hero",
            new BattleActionChoice(BattleActionKind.Attack, null, Array.Empty<string>()));

        Assert.AreEqual(0, resolution.Operations.Count);
    }

    [TestMethod]
    public void Resolve_Should_Skip_Invalid_Attack_Targets_Gracefully()
    {
        var resolver = new DefaultBattleActionResolver();

        BattleResolution resolution = resolver.Resolve(
            CreateDefaultState(),
            "hero",
            new BattleActionChoice(BattleActionKind.Attack, null, new[] { "missing", "hero", "slime" }));

        Assert.AreEqual(1, resolution.Operations.Count);
        var damage = (DamageOperation)resolution.Operations.Single();
        Assert.AreEqual("slime", damage.TargetId);
    }

    [TestMethod]
    public void Resolve_Should_Return_Defend_Operation_For_Defend()
    {
        var resolver = new DefaultBattleActionResolver();

        BattleResolution resolution = resolver.Resolve(
            CreateDefaultState(),
            "hero",
            new BattleActionChoice(BattleActionKind.Defend, null, null));

        Assert.AreEqual(1, resolution.Operations.Count);
        Assert.IsInstanceOfType<DefendAppliedOperation>(resolution.Operations.Single());
    }

    [TestMethod]
    public void Resolve_Should_Return_Empty_Resolution_For_Escape_Base_Action()
    {
        var resolver = new DefaultBattleActionResolver();

        BattleResolution resolution = resolver.Resolve(
            CreateDefaultState(),
            "hero",
            new BattleActionChoice(BattleActionKind.Escape, null, null));

        Assert.AreEqual(0, resolution.Operations.Count);
    }

    [TestMethod]
    public void Resolve_Should_Return_Empty_Resolution_For_Wait()
    {
        var resolver = new DefaultBattleActionResolver();

        BattleResolution resolution = resolver.Resolve(
            CreateDefaultState(),
            "hero",
            new BattleActionChoice(BattleActionKind.Wait, null, null));

        Assert.AreEqual(0, resolution.Operations.Count);
    }

    private static BattleState CreateDefaultState()
    {
        return new BattleState(
        [
            new BattleActorState("hero", "Hero", BattleTeam.Party, 10, 10),
            new BattleActorState("slime", "Slime", BattleTeam.Enemy, 10, 10)
        ]);
    }
}