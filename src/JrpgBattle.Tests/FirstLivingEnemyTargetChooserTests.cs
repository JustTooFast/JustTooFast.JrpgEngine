// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Ai;

[TestClass]
public sealed class FirstLivingEnemyTargetChooserTests
{
    [TestMethod]
    public void ChooseTargetId_Should_Return_First_Living_Opponent()
    {
        var chooser = new FirstLivingEnemyTargetChooser();

        var state = new BattleState(
        [
            new BattleCombatantState("hero_1", "Hero 1", BattleTeam.Party, 0, 10),
            new BattleCombatantState("hero_2", "Hero 2", BattleTeam.Party, 10, 10),
            new BattleCombatantState("slime_1", "Slime 1", BattleTeam.Enemy, 10, 10)
        ],
        isEnded: false,
        outcome: BattleOutcome.None);

        string targetId = chooser.ChooseTargetId(state, "slime_1");

        Assert.AreEqual("hero_2", targetId);
    }

    [TestMethod]
    public void ChooseTargetId_Should_Work_For_Party_Actor()
    {
        var chooser = new FirstLivingEnemyTargetChooser();

        var state = new BattleState(
        [
            new BattleCombatantState("hero_1", "Hero 1", BattleTeam.Party, 10, 10),
            new BattleCombatantState("slime_1", "Slime 1", BattleTeam.Enemy, 0, 10),
            new BattleCombatantState("slime_2", "Slime 2", BattleTeam.Enemy, 10, 10)
        ],
        isEnded: false,
        outcome: BattleOutcome.None);

        string targetId = chooser.ChooseTargetId(state, "hero_1");

        Assert.AreEqual("slime_2", targetId);
    }

    [TestMethod]
    public void ChooseTargetId_Should_Throw_When_State_Is_Null()
    {
        var chooser = new FirstLivingEnemyTargetChooser();

        Assert.ThrowsException<ArgumentNullException>(() => chooser.ChooseTargetId(null!, "hero_1"));
    }

    [TestMethod]
    public void ChooseTargetId_Should_Throw_When_ActorId_Is_Missing()
    {
        var chooser = new FirstLivingEnemyTargetChooser();

        var state = new BattleState(
        [
            new BattleCombatantState("hero_1", "Hero 1", BattleTeam.Party, 10, 10)
        ],
        isEnded: false,
        outcome: BattleOutcome.None);

        Assert.ThrowsException<ArgumentException>(() => chooser.ChooseTargetId(state, ""));
    }

    [TestMethod]
    public void ChooseTargetId_Should_Throw_When_Actor_Not_Found()
    {
        var chooser = new FirstLivingEnemyTargetChooser();

        var state = new BattleState(
        [
            new BattleCombatantState("hero_1", "Hero 1", BattleTeam.Party, 10, 10)
        ],
        isEnded: false,
        outcome: BattleOutcome.None);

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
            () => chooser.ChooseTargetId(state, "missing"));

        Assert.AreEqual("Actor 'missing' not found.", ex.Message);
    }

    [TestMethod]
    public void ChooseTargetId_Should_Throw_When_Actor_Is_Not_Alive()
    {
        var chooser = new FirstLivingEnemyTargetChooser();

        var state = new BattleState(
        [
            new BattleCombatantState("hero_1", "Hero 1", BattleTeam.Party, 0, 10),
            new BattleCombatantState("slime_1", "Slime 1", BattleTeam.Enemy, 10, 10)
        ],
        isEnded: false,
        outcome: BattleOutcome.None);

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
            () => chooser.ChooseTargetId(state, "hero_1"));

        Assert.AreEqual("Actor is not alive.", ex.Message);
    }

    [TestMethod]
    public void ChooseTargetId_Should_Throw_When_No_Valid_Target_Found()
    {
        var chooser = new FirstLivingEnemyTargetChooser();

        var state = new BattleState(
        [
            new BattleCombatantState("hero_1", "Hero 1", BattleTeam.Party, 10, 10),
            new BattleCombatantState("slime_1", "Slime 1", BattleTeam.Enemy, 0, 10)
        ],
        isEnded: false,
        outcome: BattleOutcome.None);

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
            () => chooser.ChooseTargetId(state, "hero_1"));

        Assert.AreEqual("No valid target found.", ex.Message);
    }
}