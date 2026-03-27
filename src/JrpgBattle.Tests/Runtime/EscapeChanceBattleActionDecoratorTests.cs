// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
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

        BattleResolution resolution = resolver.Resolve(
            CreateState(),
            "hero",
            new BattleActionChoice(BattleActionKind.Attack, null, new[] { "slime" }));

        Assert.AreEqual(1, resolution.Operations.Count);
        var damage = (DamageOperation)resolution.Operations.Single();
        Assert.AreEqual("slime", damage.TargetId);
        Assert.AreEqual(5, damage.Amount);
    }

    [TestMethod]
    public void Resolve_Should_Always_Fail_Escape_When_Chance_Is_Zero()
    {
        IBattleActionResolver resolver = new EscapeChanceBattleActionDecorator(
            new DefaultBattleActionResolver(),
            escapeSuccessChance: 0.0,
            seed: 123);

        BattleResolution resolution = resolver.Resolve(
            CreateState(),
            "hero",
            new BattleActionChoice(BattleActionKind.Escape, null, null));

        Assert.AreEqual(1, resolution.Operations.Count);
        Assert.IsInstanceOfType<EscapeFailedOperation>(resolution.Operations.Single());
    }

    [TestMethod]
    public void Resolve_Should_Always_Succeed_Escape_When_Chance_Is_One()
    {
        IBattleActionResolver resolver = new EscapeChanceBattleActionDecorator(
            new DefaultBattleActionResolver(),
            escapeSuccessChance: 1.0,
            seed: 123);

        BattleResolution resolution = resolver.Resolve(
            CreateState(),
            "hero",
            new BattleActionChoice(BattleActionKind.Escape, null, null));

        Assert.AreEqual(1, resolution.Operations.Count);
        Assert.IsInstanceOfType<EscapeSucceededOperation>(resolution.Operations.Single());
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

        var results1 = new[]
        {
            resolver1.Resolve(CreateState(), "hero", new BattleActionChoice(BattleActionKind.Escape, null, null)).Operations.Single().GetType(),
            resolver1.Resolve(CreateState(), "hero", new BattleActionChoice(BattleActionKind.Escape, null, null)).Operations.Single().GetType(),
            resolver1.Resolve(CreateState(), "hero", new BattleActionChoice(BattleActionKind.Escape, null, null)).Operations.Single().GetType()
        };

        var results2 = new[]
        {
            resolver2.Resolve(CreateState(), "hero", new BattleActionChoice(BattleActionKind.Escape, null, null)).Operations.Single().GetType(),
            resolver2.Resolve(CreateState(), "hero", new BattleActionChoice(BattleActionKind.Escape, null, null)).Operations.Single().GetType(),
            resolver2.Resolve(CreateState(), "hero", new BattleActionChoice(BattleActionKind.Escape, null, null)).Operations.Single().GetType()
        };

        CollectionAssert.AreEqual(results1, results2);
    }

    private static BattleState CreateState() => new(
    [
        new BattleActorState("hero", "Hero", BattleTeam.Party, 10, 10),
        new BattleActorState("slime", "Slime", BattleTeam.Enemy, 10, 10)
    ]);
}