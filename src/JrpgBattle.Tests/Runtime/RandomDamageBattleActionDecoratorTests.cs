// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System.Linq;
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
        IBattleActionResolver resolver1 = new RandomDamageBattleActionDecorator(
            new DefaultBattleActionResolver(), 1, 5, 0.0, 123);
        IBattleActionResolver resolver2 = new RandomDamageBattleActionDecorator(
            new DefaultBattleActionResolver(), 1, 5, 0.0, 123);

        int[] results1 =
        [
            ((DamageOperation)resolver1.Resolve(CreateState(), "hero", CreateAction()).Operations.Single()).Amount,
            ((DamageOperation)resolver1.Resolve(CreateState(), "hero", CreateAction()).Operations.Single()).Amount
        ];

        int[] results2 =
        [
            ((DamageOperation)resolver2.Resolve(CreateState(), "hero", CreateAction()).Operations.Single()).Amount,
            ((DamageOperation)resolver2.Resolve(CreateState(), "hero", CreateAction()).Operations.Single()).Amount
        ];

        CollectionAssert.AreEqual(results1, results2);
    }

    [TestMethod]
    public void Resolve_Should_Remove_Damage_When_MissChance_Is_One()
    {
        IBattleActionResolver resolver = new RandomDamageBattleActionDecorator(
            new DefaultBattleActionResolver(), 1, 5, 1.0, 123);

        BattleResolution resolution = resolver.Resolve(CreateState(), "hero", CreateAction());

        Assert.AreEqual(0, resolution.Operations.Count);
    }

    private static BattleState CreateState() => new(
    [
        new BattleActorState("hero", "Hero", BattleTeam.Party, 10, 10),
        new BattleActorState("slime", "Slime", BattleTeam.Enemy, 10, 10)
    ]);

    private static BattleActionChoice CreateAction() =>
        new(
            BattleActionKind.Attack,
            null,
            BattleTargetMode.SingleTarget,
            new[] { "slime" });
}