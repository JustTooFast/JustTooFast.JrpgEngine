// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Runtime;

[TestClass]
public sealed class AlwaysBlockOnPlayerChoicePolicyTests
{
    [TestMethod]
    public void ShouldBlockAdvance_Should_Return_True()
    {
        var policy = new AlwaysBlockOnPlayerChoicePolicy();

        var runtimeView = new BattleRuntimeView(
            new BattleState(
            [
                new BattleCombatantState("hero", "Hero", BattleTeam.Party, 10, 10)
            ],
            isEnded: false,
            outcome: BattleOutcome.None),
            pendingPlayerActorId: null);

        bool result = policy.ShouldBlockAdvance(runtimeView);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ShouldBlockAdvance_Should_Throw_When_RuntimeView_Is_Null()
    {
        var policy = new AlwaysBlockOnPlayerChoicePolicy();

        Assert.ThrowsException<ArgumentNullException>(() => policy.ShouldBlockAdvance(null!));
    }
}