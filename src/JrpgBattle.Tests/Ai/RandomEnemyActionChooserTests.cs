// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Ai;

[TestClass]
public sealed class RandomEnemyActionChooserTests
{
    [TestMethod]
    public void ChooseAction_Should_Be_Deterministic_With_Seed()
    {
        var chooser1 = new RandomEnemyActionChooser(123);
        var chooser2 = new RandomEnemyActionChooser(123);

        var state = new BattleState(
        [
            new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 10, 10)
        ],
        false,
        BattleOutcome.None);

        var results1 = new[]
        {
            chooser1.ChooseAction(state, "slime"),
            chooser1.ChooseAction(state, "slime"),
            chooser1.ChooseAction(state, "slime")
        };

        var results2 = new[]
        {
            chooser2.ChooseAction(state, "slime"),
            chooser2.ChooseAction(state, "slime"),
            chooser2.ChooseAction(state, "slime")
        };

        CollectionAssert.AreEqual(results1, results2);
    }
}