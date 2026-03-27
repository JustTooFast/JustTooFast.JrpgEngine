// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Flow;

[TestClass]
public sealed class TeamPhaseBattleFlowTests
{
    [TestMethod]
    public void Advance_Should_Run_Whole_Party_Phase_First_When_StartingTeam_Is_Party()
    {
        var flow = new TeamPhaseBattleFlow(BattleTeam.Party);
        var state = CreateState(
            new("hero_1", "Hero 1", BattleTeam.Party, 10, 10),
            new("hero_2", "Hero 2", BattleTeam.Party, 10, 10),
            new("slime_1", "Slime 1", BattleTeam.Enemy, 10, 10),
            new("slime_2", "Slime 2", BattleTeam.Enemy, 10, 10));

        Assert.AreEqual("hero_1", flow.Advance(state).ReadyActorId);
        Assert.AreEqual("hero_2", flow.Advance(state).ReadyActorId);
        Assert.AreEqual("slime_1", flow.Advance(state).ReadyActorId);
        Assert.AreEqual("slime_2", flow.Advance(state).ReadyActorId);
        Assert.AreEqual("hero_1", flow.Advance(state).ReadyActorId);
    }

    [TestMethod]
    public void Advance_Should_Switch_To_Other_Team_When_Current_Team_Has_No_Living_Actors()
    {
        var flow = new TeamPhaseBattleFlow(BattleTeam.Party);
        var state = CreateState(
            new("hero_1", "Hero 1", BattleTeam.Party, 0, 10),
            new("hero_2", "Hero 2", BattleTeam.Party, 0, 10),
            new("slime_1", "Slime 1", BattleTeam.Enemy, 10, 10),
            new("slime_2", "Slime 2", BattleTeam.Enemy, 10, 10));

        BattleFlowStep step = flow.Advance(state);

        Assert.IsTrue(step.HasAdvanced);
        Assert.AreEqual("slime_1", step.ReadyActorId);
    }

    [TestMethod]
    public void Advance_Should_Return_No_Ready_Actor_When_No_Living_Combatants_Exist()
    {
        var flow = new TeamPhaseBattleFlow(BattleTeam.Party);
        var state = CreateState(
            new("hero_1", "Hero 1", BattleTeam.Party, 0, 10),
            new("slime_1", "Slime 1", BattleTeam.Enemy, 0, 10));

        BattleFlowStep step = flow.Advance(state);

        Assert.IsFalse(step.HasAdvanced);
        Assert.IsNull(step.ReadyActorId);
    }

    [TestMethod]
    public void Advance_Should_Throw_When_State_Is_Null()
    {
        var flow = new TeamPhaseBattleFlow(BattleTeam.Party);

        Assert.ThrowsException<ArgumentNullException>(() => flow.Advance(null!));
    }

    private static BattleState CreateState(params BattleCombatantState[] combatants) => new(combatants);
}