// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Ai;

[TestClass]
public sealed class AlwaysEscapeEnemyActionChooserTests
{
    [TestMethod]
    public void ChooseAction_Should_Return_Escape()
    {
        var chooser = new AlwaysEscapeEnemyActionChooser();

        var state = new BattleState(
        [
            new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 10, 10)
        ],
        false,
        BattleOutcome.None);

        BattleActionKind action = chooser.ChooseAction(state, "slime");

        Assert.AreEqual(BattleActionKind.Escape, action);
    }

    [TestMethod]
    public void ChooseAction_Should_Throw_When_State_Is_Null()
    {
        var chooser = new AlwaysEscapeEnemyActionChooser();

        Assert.ThrowsException<ArgumentNullException>(() => chooser.ChooseAction(null!, "slime"));
    }
}