// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
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
        BattleFlowState state = CreateFlowState(
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
    public void Advance_Should_Switch_To_Other_Team_When_Current_Team_Has_No_Actors_Who_Can_Act()
    {
        var flow = new TeamPhaseBattleFlow(BattleTeam.Party);
        BattleFlowState state = CreateFlowStateWithPrevented(
            new[] { "hero_1", "hero_2" },
            new("hero_1", "Hero 1", BattleTeam.Party, 10, 10),
            new("hero_2", "Hero 2", BattleTeam.Party, 10, 10),
            new("slime_1", "Slime 1", BattleTeam.Enemy, 10, 10),
            new("slime_2", "Slime 2", BattleTeam.Enemy, 10, 10));

        BattleFlowStep step = flow.Advance(state);

        Assert.IsTrue(step.HasAdvanced);
        Assert.AreEqual("slime_1", step.ReadyActorId);
    }

    [TestMethod]
    public void Advance_Should_Skip_Actors_Prevented_From_Acting_Within_A_Team_Phase()
    {
        var flow = new TeamPhaseBattleFlow(BattleTeam.Party);
        BattleFlowState state = CreateFlowStateWithPrevented(
            new[] { "hero_2", "slime_2" },
            new("hero_1", "Hero 1", BattleTeam.Party, 10, 10),
            new("hero_2", "Hero 2", BattleTeam.Party, 10, 10),
            new("slime_1", "Slime 1", BattleTeam.Enemy, 10, 10),
            new("slime_2", "Slime 2", BattleTeam.Enemy, 10, 10));

        Assert.AreEqual("hero_1", flow.Advance(state).ReadyActorId);
        Assert.AreEqual("slime_1", flow.Advance(state).ReadyActorId);
        Assert.AreEqual("hero_1", flow.Advance(state).ReadyActorId);
    }

    [TestMethod]
    public void Advance_Should_Return_No_Ready_Actor_When_No_Actors_Can_Act()
    {
        var flow = new TeamPhaseBattleFlow(BattleTeam.Party);
        BattleFlowState state = CreateFlowStateWithPrevented(
            new[] { "hero_1", "slime_1" },
            new("hero_1", "Hero 1", BattleTeam.Party, 10, 10),
            new("slime_1", "Slime 1", BattleTeam.Enemy, 10, 10));

        BattleFlowStep step = flow.Advance(state);

        Assert.IsFalse(step.HasAdvanced);
        Assert.IsNull(step.ReadyActorId);
    }

    [TestMethod]
    public void Advance_Should_Return_No_Ready_Actor_When_No_Living_Actors_Exist()
    {
        var flow = new TeamPhaseBattleFlow(BattleTeam.Party);
        BattleFlowState state = CreateFlowState(
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

    private static BattleFlowState CreateFlowState(params BattleActorState[] actors)
        => CreateFlowStateWithPrevented(Array.Empty<string>(), actors);

    private static BattleFlowState CreateFlowStateWithPrevented(
        IReadOnlyCollection<string> preventedActorIds,
        params BattleActorState[] actors)
    {
        var battleState = new BattleState(actors);

        var actorStates = new Dictionary<string, BattleFlowActorState>(StringComparer.Ordinal);
        foreach (BattleActorState actor in actors)
        {
            bool preventsActing = preventedActorIds.Contains(actor.Id, StringComparer.Ordinal);

            actorStates[actor.Id] = new BattleFlowActorState(
                actorId: actor.Id,
                isDefeated: actor.IsDefeated,
                isDefending: false,
                preventsActing: preventsActing);
        }

        return new BattleFlowState(battleState, actorStates);
    }
}