// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Flow;

[TestClass]
public sealed class FirstLivingBattleFlowTests
{
    [TestMethod]
    public void Advance_Should_Return_First_Living_Combatant()
    {
        var flow = new FirstLivingBattleFlow();

        var state = new BattleState(
        [
            new BattleCombatantState("hero_1", "Hero 1", BattleTeam.Party, 0, 10),
            new BattleCombatantState("hero_2", "Hero 2", BattleTeam.Party, 10, 10),
            new BattleCombatantState("slime_1", "Slime 1", BattleTeam.Enemy, 10, 10)
        ],
        isEnded: false,
        outcome: BattleOutcome.None);

        BattleFlowResult result = flow.Advance(state);

        Assert.IsTrue(result.HasChanged);
        Assert.AreEqual("hero_2", result.ReadyActorId);
    }

    [TestMethod]
    public void Advance_Should_Return_Same_ReadyActor_When_Not_Consumed()
    {
        var flow = new FirstLivingBattleFlow();

        var state = new BattleState(
        [
            new BattleCombatantState("hero_1", "Hero 1", BattleTeam.Party, 10, 10),
            new BattleCombatantState("slime_1", "Slime 1", BattleTeam.Enemy, 10, 10)
        ],
        isEnded: false,
        outcome: BattleOutcome.None);

        BattleFlowResult first = flow.Advance(state);
        BattleFlowResult second = flow.Advance(state);

        Assert.IsTrue(first.HasChanged);
        Assert.AreEqual("hero_1", first.ReadyActorId);

        Assert.IsFalse(second.HasChanged);
        Assert.AreEqual("hero_1", second.ReadyActorId);
    }

    [TestMethod]
    public void Advance_Should_Return_NoReadyActor_When_No_Living_Combatants_Exist()
    {
        var flow = new FirstLivingBattleFlow();

        var state = new BattleState(
        [
            new BattleCombatantState("hero_1", "Hero 1", BattleTeam.Party, 0, 10),
            new BattleCombatantState("slime_1", "Slime 1", BattleTeam.Enemy, 0, 10)
        ],
        isEnded: false,
        outcome: BattleOutcome.None);

        BattleFlowResult result = flow.Advance(state);

        Assert.IsFalse(result.HasChanged);
        Assert.IsNull(result.ReadyActorId);
    }

    [TestMethod]
    public void Advance_Should_Throw_When_State_Is_Null()
    {
        var flow = new FirstLivingBattleFlow();

        Assert.ThrowsException<ArgumentNullException>(() => flow.Advance(null!));
    }

    [TestMethod]
    public void ConsumeReadyActor_Should_Clear_Current_Ready_Actor()
    {
        var flow = new FirstLivingBattleFlow();

        var state = new BattleState(
        [
            new BattleCombatantState("hero_1", "Hero 1", BattleTeam.Party, 10, 10)
        ],
        isEnded: false,
        outcome: BattleOutcome.None);

        BattleFlowResult first = flow.Advance(state);
        flow.ConsumeReadyActor(first.ReadyActorId!);
        BattleFlowResult second = flow.Advance(state);

        Assert.AreEqual("hero_1", second.ReadyActorId);
        Assert.IsTrue(second.HasChanged);
    }

    [TestMethod]
    public void ConsumeReadyActor_Should_Throw_When_ActorId_Is_Missing()
    {
        var flow = new FirstLivingBattleFlow();

        Assert.ThrowsException<ArgumentException>(() => flow.ConsumeReadyActor(""));
    }

    [TestMethod]
    public void ConsumeReadyActor_Should_Throw_When_ActorId_Does_Not_Match_Current_Ready_Actor()
    {
        var flow = new FirstLivingBattleFlow();

        var state = new BattleState(
        [
            new BattleCombatantState("hero_1", "Hero 1", BattleTeam.Party, 10, 10)
        ],
        isEnded: false,
        outcome: BattleOutcome.None);

        _ = flow.Advance(state);

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
            () => flow.ConsumeReadyActor("someone_else"));

        Assert.AreEqual("Ready actor does not match the actor being consumed.", ex.Message);
    }
}