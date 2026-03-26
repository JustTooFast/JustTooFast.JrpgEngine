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

        BattleFlowResult step1 = flow.Advance(state);
        Assert.AreEqual("hero_1", step1.ReadyActorId);
        flow.ConsumeReadyActor(step1.ReadyActorId!);

        BattleFlowResult step2 = flow.Advance(state);
        Assert.AreEqual("hero_2", step2.ReadyActorId);
        flow.ConsumeReadyActor(step2.ReadyActorId!);

        BattleFlowResult step3 = flow.Advance(state);
        Assert.AreEqual("slime_1", step3.ReadyActorId);
        flow.ConsumeReadyActor(step3.ReadyActorId!);

        BattleFlowResult step4 = flow.Advance(state);
        Assert.AreEqual("slime_2", step4.ReadyActorId);
    }

    [TestMethod]
    public void Advance_Should_Run_Whole_Enemy_Phase_First_When_StartingTeam_Is_Enemy()
    {
        var flow = new TeamPhaseBattleFlow(BattleTeam.Enemy);
        var state = CreateState(
            new("hero_1", "Hero 1", BattleTeam.Party, 10, 10),
            new("hero_2", "Hero 2", BattleTeam.Party, 10, 10),
            new("slime_1", "Slime 1", BattleTeam.Enemy, 10, 10),
            new("slime_2", "Slime 2", BattleTeam.Enemy, 10, 10));

        BattleFlowResult step1 = flow.Advance(state);
        Assert.AreEqual("slime_1", step1.ReadyActorId);
        flow.ConsumeReadyActor(step1.ReadyActorId!);

        BattleFlowResult step2 = flow.Advance(state);
        Assert.AreEqual("slime_2", step2.ReadyActorId);
        flow.ConsumeReadyActor(step2.ReadyActorId!);

        BattleFlowResult step3 = flow.Advance(state);
        Assert.AreEqual("hero_1", step3.ReadyActorId);
        flow.ConsumeReadyActor(step3.ReadyActorId!);

        BattleFlowResult step4 = flow.Advance(state);
        Assert.AreEqual("hero_2", step4.ReadyActorId);
    }

    [TestMethod]
    public void Advance_Should_Skip_Dead_Actors_Within_A_Phase()
    {
        var flow = new TeamPhaseBattleFlow(BattleTeam.Party);
        var state = CreateState(
            new("hero_1", "Hero 1", BattleTeam.Party, 10, 10),
            new("hero_2", "Hero 2", BattleTeam.Party, 0, 10),
            new("slime_1", "Slime 1", BattleTeam.Enemy, 10, 10),
            new("slime_2", "Slime 2", BattleTeam.Enemy, 0, 10));

        BattleFlowResult step1 = flow.Advance(state);
        Assert.AreEqual("hero_1", step1.ReadyActorId);
        flow.ConsumeReadyActor(step1.ReadyActorId!);

        BattleFlowResult step2 = flow.Advance(state);
        Assert.AreEqual("slime_1", step2.ReadyActorId);
        flow.ConsumeReadyActor(step2.ReadyActorId!);

        BattleFlowResult step3 = flow.Advance(state);
        Assert.AreEqual("hero_1", step3.ReadyActorId);
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

        BattleFlowResult result = flow.Advance(state);

        Assert.IsTrue(result.HasChanged);
        Assert.AreEqual("slime_1", result.ReadyActorId);
    }

    [TestMethod]
    public void Advance_Should_Return_Same_ReadyActor_When_Not_Consumed()
    {
        var flow = new TeamPhaseBattleFlow(BattleTeam.Party);
        var state = CreateState(
            new("hero_1", "Hero 1", BattleTeam.Party, 10, 10),
            new("slime_1", "Slime 1", BattleTeam.Enemy, 10, 10));

        BattleFlowResult first = flow.Advance(state);
        BattleFlowResult second = flow.Advance(state);

        Assert.IsTrue(first.HasChanged);
        Assert.AreEqual("hero_1", first.ReadyActorId);

        Assert.IsFalse(second.HasChanged);
        Assert.AreEqual("hero_1", second.ReadyActorId);
    }

    [TestMethod]
    public void Advance_Should_Loop_Back_To_Starting_Phase_Pattern()
    {
        var flow = new TeamPhaseBattleFlow(BattleTeam.Party);
        var state = CreateState(
            new("hero_1", "Hero 1", BattleTeam.Party, 10, 10),
            new("slime_1", "Slime 1", BattleTeam.Enemy, 10, 10));

        BattleFlowResult step1 = flow.Advance(state);
        Assert.AreEqual("hero_1", step1.ReadyActorId);
        flow.ConsumeReadyActor(step1.ReadyActorId!);

        BattleFlowResult step2 = flow.Advance(state);
        Assert.AreEqual("slime_1", step2.ReadyActorId);
        flow.ConsumeReadyActor(step2.ReadyActorId!);

        BattleFlowResult step3 = flow.Advance(state);
        Assert.AreEqual("hero_1", step3.ReadyActorId);
    }

    [TestMethod]
    public void Advance_Should_Return_NoReadyActor_When_No_Living_Combatants_Exist()
    {
        var flow = new TeamPhaseBattleFlow(BattleTeam.Party);
        var state = CreateState(
            new("hero_1", "Hero 1", BattleTeam.Party, 0, 10),
            new("slime_1", "Slime 1", BattleTeam.Enemy, 0, 10));

        BattleFlowResult result = flow.Advance(state);

        Assert.IsFalse(result.HasChanged);
        Assert.IsNull(result.ReadyActorId);
    }

    [TestMethod]
    public void Advance_Should_Throw_When_State_Is_Null()
    {
        var flow = new TeamPhaseBattleFlow(BattleTeam.Party);

        Assert.ThrowsException<ArgumentNullException>(() => flow.Advance(null!));
    }

    [TestMethod]
    public void ConsumeReadyActor_Should_Throw_When_ActorId_Is_Missing()
    {
        var flow = new TeamPhaseBattleFlow(BattleTeam.Party);

        Assert.ThrowsException<ArgumentException>(() => flow.ConsumeReadyActor(""));
    }

    [TestMethod]
    public void ConsumeReadyActor_Should_Throw_When_ActorId_Does_Not_Match_Current_Ready_Actor()
    {
        var flow = new TeamPhaseBattleFlow(BattleTeam.Party);
        var state = CreateState(
            new("hero_1", "Hero 1", BattleTeam.Party, 10, 10),
            new("slime_1", "Slime 1", BattleTeam.Enemy, 10, 10));

        _ = flow.Advance(state);

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
            () => flow.ConsumeReadyActor("someone_else"));

        Assert.AreEqual("Ready actor does not match the actor being consumed.", ex.Message);
    }

    private static BattleState CreateState(params BattleCombatantState[] combatants)
    {
        return new BattleState(
            combatants,
            isEnded: false,
            outcome: BattleOutcome.None);
    }
}