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
                new("dead", "Dead", BattleTeam.Party, 0, 10),
                new("alive", "Alive", BattleTeam.Party, 10, 10)
            ],
            false,
            BattleOutcome.None);

        var result = flow.Advance(state);

        Assert.IsTrue(result.HasChanged);
        Assert.AreEqual("alive", result.ReadyActorId);
    }

    [TestMethod]
    public void ConsumeReadyActor_Should_Clear_Ready_Actor()
    {
        var flow = new FirstLivingBattleFlow();

        var state = new BattleState(
            [
                new("hero", "Hero", BattleTeam.Party, 10, 10)
            ],
            false,
            BattleOutcome.None);

        var result = flow.Advance(state);
        flow.ConsumeReadyActor(result.ReadyActorId!);

        var next = flow.Advance(state);

        Assert.IsTrue(next.HasChanged);
        Assert.AreEqual("hero", next.ReadyActorId);
    }
}