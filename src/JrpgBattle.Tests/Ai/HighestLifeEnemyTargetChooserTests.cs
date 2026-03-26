// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Ai;

[TestClass]
public sealed class HighestLifeEnemyTargetChooserTests
{
    [TestMethod]
    public void ChooseTargetId_Should_Return_Highest_Life_Target()
    {
        var chooser = new HighestLifeEnemyTargetChooser();

        var state = new BattleState(
        [
            new BattleCombatantState("hero_1", "Hero 1", BattleTeam.Party, 5, 10),
            new BattleCombatantState("hero_2", "Hero 2", BattleTeam.Party, 10, 10),
            new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 10, 10)
        ],
        false,
        BattleOutcome.None);

        string target = chooser.ChooseTargetId(state, "slime");

        Assert.AreEqual("hero_2", target);
    }
}