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
        DefaultBattleActionResolver resolver = new();

        Assert.ThrowsException<ArgumentNullException>(() =>
            resolver.Resolve(
                null!,
                "hero",
                new BattleActionChoice(
                    BattleActionKind.Defend,
                    null,
                    BattleTargetMode.None,
                    null)));
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Action_Is_Null()
    {
        DefaultBattleActionResolver resolver = new();

        Assert.ThrowsException<ArgumentNullException>(() =>
            resolver.Resolve(CreateDefaultState(), "hero", null!));
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Actor_Is_Not_Found()
    {
        DefaultBattleActionResolver resolver = new();

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(() =>
            resolver.Resolve(
                CreateDefaultState(),
                "missing",
                new BattleActionChoice(
                    BattleActionKind.Defend,
                    null,
                    BattleTargetMode.None,
                    null)));

        Assert.AreEqual("Actor 'missing' not found.", ex.Message);
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Actor_Is_Defeated()
    {
        DefaultBattleActionResolver resolver = new();

        BattleState state = new(
        [
            new BattleActorState("hero", "Hero", BattleTeam.Party, 0, 10),
            new BattleActorState("slime", "Slime", BattleTeam.Enemy, 10, 10)
        ]);

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(() =>
            resolver.Resolve(
                state,
                "hero",
                new BattleActionChoice(
                    BattleActionKind.Defend,
                    null,
                    BattleTargetMode.None,
                    null)));

        Assert.AreEqual("Actor is defeated.", ex.Message);
    }

    [TestMethod]
    public void Resolve_Should_Return_Damage_Operation_For_Valid_Attack_Target()
    {
        DefaultBattleActionResolver resolver = new();

        BattleResolution resolution = resolver.Resolve(
            CreateDefaultState(),
            "hero",
            new BattleActionChoice(
                BattleActionKind.Attack,
                null,
                BattleTargetMode.SingleTarget,
                new[] { "slime" }));

        Assert.AreEqual(1, resolution.Operations.Count);
        Assert.IsInstanceOfType<DamageOperation>(resolution.Operations.Single());

        DamageOperation damage = (DamageOperation)resolution.Operations.Single();
        Assert.AreEqual("slime", damage.TargetId);
        Assert.AreEqual(1, damage.Amount);
    }

    [TestMethod]
    public void Resolve_Should_Return_Empty_Resolution_For_Attack_When_Target_Is_Not_Found()
    {
        DefaultBattleActionResolver resolver = new();

        BattleResolution resolution = resolver.Resolve(
            CreateDefaultState(),
            "hero",
            new BattleActionChoice(
                BattleActionKind.Attack,
                null,
                BattleTargetMode.SingleTarget,
                new[] { "missing" }));

        Assert.AreEqual(0, resolution.Operations.Count);
    }

    [TestMethod]
    public void Resolve_Should_Return_Empty_Resolution_For_Attack_When_Target_Is_On_Same_Team()
    {
        DefaultBattleActionResolver resolver = new();

        BattleResolution resolution = resolver.Resolve(
            CreateDefaultState(),
            "hero",
            new BattleActionChoice(
                BattleActionKind.Attack,
                null,
                BattleTargetMode.SingleTarget,
                new[] { "hero" }));

        Assert.AreEqual(0, resolution.Operations.Count);
    }

    [TestMethod]
    public void Resolve_Should_Return_Empty_Resolution_For_Attack_When_Target_Is_Defeated()
    {
        DefaultBattleActionResolver resolver = new();

        BattleState state = new(
        [
            new BattleActorState("hero", "Hero", BattleTeam.Party, 10, 10),
            new BattleActorState("slime", "Slime", BattleTeam.Enemy, 0, 10)
        ]);

        BattleResolution resolution = resolver.Resolve(
            state,
            "hero",
            new BattleActionChoice(
                BattleActionKind.Attack,
                null,
                BattleTargetMode.SingleTarget,
                new[] { "slime" }));

        Assert.AreEqual(0, resolution.Operations.Count);
    }

    [TestMethod]
    public void Resolve_Should_Return_Defend_Operation_For_Defend()
    {
        DefaultBattleActionResolver resolver = new();

        BattleResolution resolution = resolver.Resolve(
            CreateDefaultState(),
            "hero",
            new BattleActionChoice(
                BattleActionKind.Defend,
                null,
                BattleTargetMode.None,
                null));

        Assert.AreEqual(1, resolution.Operations.Count);
        Assert.IsInstanceOfType<DefendAppliedOperation>(resolution.Operations.Single());
    }

    [TestMethod]
    public void Resolve_Should_Return_Empty_Resolution_For_Escape_Base_Action()
    {
        DefaultBattleActionResolver resolver = new();

        BattleResolution resolution = resolver.Resolve(
            CreateDefaultState(),
            "hero",
            new BattleActionChoice(
                BattleActionKind.Escape,
                null,
                BattleTargetMode.None,
                null));

        Assert.AreEqual(0, resolution.Operations.Count);
    }

    [TestMethod]
    public void Resolve_Should_Return_Empty_Resolution_For_Wait()
    {
        DefaultBattleActionResolver resolver = new();

        BattleResolution resolution = resolver.Resolve(
            CreateDefaultState(),
            "hero",
            new BattleActionChoice(
                BattleActionKind.Wait,
                null,
                BattleTargetMode.None,
                null));

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