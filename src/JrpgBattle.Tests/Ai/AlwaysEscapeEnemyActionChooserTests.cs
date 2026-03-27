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
        ]);

        BattleActionChoice choice = chooser.ChooseAction(state, "slime");

        Assert.AreEqual(BattleActionKind.Escape, choice.ActionKind);
        Assert.IsNull(choice.ActionId);
        Assert.IsNull(choice.TargetIds);
    }

    [TestMethod]
    public void ChooseAction_Should_Throw_When_State_Is_Null()
    {
        var chooser = new AlwaysEscapeEnemyActionChooser();

        Assert.ThrowsException<ArgumentNullException>(() => chooser.ChooseAction(null!, "slime"));
    }

    [TestMethod]
    public void ChooseAction_Should_Throw_When_Actor_Is_Not_Found()
    {
        var chooser = new AlwaysEscapeEnemyActionChooser();

        var state = new BattleState(
        [
            new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 10, 10)
        ]);

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
            () => chooser.ChooseAction(state, "missing"));

        Assert.AreEqual("Actor 'missing' not found.", ex.Message);
    }
}