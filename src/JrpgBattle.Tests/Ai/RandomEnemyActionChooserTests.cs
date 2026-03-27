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
            new BattleActorState("slime", "Slime", BattleTeam.Enemy, 10, 10),
            new BattleActorState("hero_1", "Hero 1", BattleTeam.Party, 10, 10),
            new BattleActorState("hero_2", "Hero 2", BattleTeam.Party, 10, 10)
        ]);

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

    [TestMethod]
    public void ChooseAction_Should_Fall_Back_To_Defend_When_No_Living_Targets_Exist()
    {
        var chooser = new RandomEnemyActionChooser(123);

        var state = new BattleState(
        [
            new BattleActorState("slime", "Slime", BattleTeam.Enemy, 10, 10),
            new BattleActorState("hero_1", "Hero 1", BattleTeam.Party, 0, 10)
        ]);

        BattleActionChoice choice = chooser.ChooseAction(state, "slime");

        Assert.AreEqual(BattleActionKind.Defend, choice.ActionKind);
        Assert.IsNull(choice.TargetIds);
    }
}