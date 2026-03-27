// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Ai;

[TestClass]
public sealed class AlwaysAttackEnemyActionChooserTests
{
    [TestMethod]
    public void ChooseAction_Should_Return_Attack_With_First_Living_Opponent_Target()
    {
        var chooser = new AlwaysAttackEnemyActionChooser();

        var state = new BattleState(
        [
            new BattleCombatantState("slime_1", "Slime 1", BattleTeam.Enemy, 10, 10),
            new BattleCombatantState("hero_1", "Hero 1", BattleTeam.Party, 10, 10)
        ]);

        BattleActionChoice choice = chooser.ChooseAction(state, "slime_1");

        Assert.AreEqual(BattleActionKind.Attack, choice.ActionKind);
        Assert.IsNull(choice.ActionId);
        Assert.IsNotNull(choice.TargetIds);
        Assert.AreEqual(1, choice.TargetIds.Count);
        Assert.AreEqual("hero_1", choice.TargetIds.Single());
    }

    [TestMethod]
    public void ChooseAction_Should_Return_Attack_With_Empty_Targets_When_No_Living_Opponent_Exists()
    {
        var chooser = new AlwaysAttackEnemyActionChooser();

        var state = new BattleState(
        [
            new BattleCombatantState("slime_1", "Slime 1", BattleTeam.Enemy, 10, 10),
            new BattleCombatantState("hero_1", "Hero 1", BattleTeam.Party, 0, 10)
        ]);

        BattleActionChoice choice = chooser.ChooseAction(state, "slime_1");

        Assert.AreEqual(BattleActionKind.Attack, choice.ActionKind);
        Assert.IsNotNull(choice.TargetIds);
        Assert.AreEqual(0, choice.TargetIds.Count);
    }

    [TestMethod]
    public void ChooseAction_Should_Throw_When_State_Is_Null()
    {
        var chooser = new AlwaysAttackEnemyActionChooser();

        Assert.ThrowsException<ArgumentNullException>(() => chooser.ChooseAction(null!, "slime_1"));
    }

    [TestMethod]
    public void ChooseAction_Should_Throw_When_ActorId_Is_Missing()
    {
        var chooser = new AlwaysAttackEnemyActionChooser();

        var state = new BattleState(
        [
            new BattleCombatantState("slime_1", "Slime 1", BattleTeam.Enemy, 10, 10)
        ]);

        Assert.ThrowsException<ArgumentException>(() => chooser.ChooseAction(state, ""));
    }

    [TestMethod]
    public void ChooseAction_Should_Throw_When_Actor_Is_Not_Found()
    {
        var chooser = new AlwaysAttackEnemyActionChooser();

        var state = new BattleState(
        [
            new BattleCombatantState("slime_1", "Slime 1", BattleTeam.Enemy, 10, 10)
        ]);

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
            () => chooser.ChooseAction(state, "missing"));

        Assert.AreEqual("Actor 'missing' not found.", ex.Message);
    }

    [TestMethod]
    public void ChooseAction_Should_Throw_When_Actor_Is_Defeated()
    {
        var chooser = new AlwaysAttackEnemyActionChooser();

        var state = new BattleState(
        [
            new BattleCombatantState("slime_1", "Slime 1", BattleTeam.Enemy, 0, 10),
            new BattleCombatantState("hero_1", "Hero 1", BattleTeam.Party, 10, 10)
        ]);

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
            () => chooser.ChooseAction(state, "slime_1"));

        Assert.AreEqual("Actor is defeated.", ex.Message);
    }
}