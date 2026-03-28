// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System.Linq;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Runtime;

[TestClass]
public sealed class FixedDamageBattleActionDecoratorTests
{
    [TestMethod]
    public void Resolve_Should_Replace_Attack_Damage_With_Configured_Value()
    {
        IBattleActionResolver resolver = new FixedDamageBattleActionDecorator(
            new DefaultBattleActionResolver(),
            5);

        BattleResolution resolution = resolver.Resolve(
            CreateState(),
            "hero",
            new BattleActionChoice(
                BattleActionKind.Attack,
                null,
                BattleTargetMode.SingleTarget,
                new[] { "slime" }));

        DamageOperation damage = (DamageOperation)resolution.Operations.Single();
        Assert.AreEqual(5, damage.Amount);
    }

    [TestMethod]
    public void Resolve_Should_Delegate_NonAttack_Actions_To_Inner_Resolver()
    {
        IBattleActionResolver resolver = new FixedDamageBattleActionDecorator(
            new DefaultBattleActionResolver(),
            5);

        BattleResolution resolution = resolver.Resolve(
            CreateState(),
            "hero",
            new BattleActionChoice(
                BattleActionKind.Wait,
                null,
                BattleTargetMode.None,
                null));

        Assert.AreEqual(0, resolution.Operations.Count);
    }

    private static BattleState CreateState() => new(
    [
        new BattleActorState("hero", "Hero", BattleTeam.Party, 10, 10),
        new BattleActorState("slime", "Slime", BattleTeam.Enemy, 10, 10)
    ]);
}