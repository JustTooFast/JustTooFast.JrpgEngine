// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Ai;

[TestClass]
public sealed class RandomEnemyTargetChooserTests
{
    [TestMethod]
    public void ChooseTargetId_Should_Be_Deterministic_With_Seed()
    {
        var chooser1 = new RandomEnemyTargetChooser(42);
        var chooser2 = new RandomEnemyTargetChooser(42);

        var state = new BattleState(
        [
            new BattleCombatantState("hero_1", "Hero 1", BattleTeam.Party, 10, 10),
            new BattleCombatantState("hero_2", "Hero 2", BattleTeam.Party, 10, 10),
            new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 10, 10)
        ],
        false,
        BattleOutcome.None);

        var results1 = new[]
        {
            chooser1.ChooseTargetId(state, "slime"),
            chooser1.ChooseTargetId(state, "slime"),
            chooser1.ChooseTargetId(state, "slime")
        };

        var results2 = new[]
        {
            chooser2.ChooseTargetId(state, "slime"),
            chooser2.ChooseTargetId(state, "slime"),
            chooser2.ChooseTargetId(state, "slime")
        };

        CollectionAssert.AreEqual(results1, results2);
    }
}