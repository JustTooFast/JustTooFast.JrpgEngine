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