// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Ai;

[TestClass]
public sealed class AlwaysDefendEnemyActionChooserTests
{
    [TestMethod]
    public void ChooseAction_Should_Return_Defend()
    {
        var chooser = new AlwaysDefendEnemyActionChooser();

        var state = new BattleState(
        [
            new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 10, 10)
        ]);

        BattleActionChoice choice = chooser.ChooseAction(state, "slime");

        Assert.AreEqual(BattleActionKind.Defend, choice.ActionKind);
        Assert.IsNull(choice.ActionId);
        Assert.IsNull(choice.TargetIds);
    }

    [TestMethod]
    public void ChooseAction_Should_Throw_When_State_Is_Null()
    {
        var chooser = new AlwaysDefendEnemyActionChooser();

        Assert.ThrowsException<ArgumentNullException>(() => chooser.ChooseAction(null!, "slime"));
    }

    [TestMethod]
    public void ChooseAction_Should_Throw_When_Actor_Is_Not_Found()
    {
        var chooser = new AlwaysDefendEnemyActionChooser();

        var state = new BattleState(
        [
            new BattleCombatantState("slime", "Slime", BattleTeam.Enemy, 10, 10)
        ]);

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
            () => chooser.ChooseAction(state, "missing"));

        Assert.AreEqual("Actor 'missing' not found.", ex.Message);
    }
}