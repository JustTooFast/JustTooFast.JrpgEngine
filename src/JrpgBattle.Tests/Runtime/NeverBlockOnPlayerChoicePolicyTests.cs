// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Runtime;

[TestClass]
public sealed class NeverBlockOnPlayerChoicePolicyTests
{
    [TestMethod]
    public void ShouldBlockAdvance_Should_Return_False()
    {
        var policy = new NeverBlockOnPlayerChoicePolicy();

        var runtimeView = new BattleRuntimeView(
            new BattleState(
            [
                new BattleCombatantState("hero", "Hero", BattleTeam.Party, 10, 10)
            ],
            isEnded: false,
            outcome: BattleOutcome.None));

        bool result = policy.ShouldBlockAdvance(runtimeView);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ShouldBlockAdvance_Should_Throw_When_RuntimeView_Is_Null()
    {
        var policy = new NeverBlockOnPlayerChoicePolicy();

        Assert.ThrowsException<ArgumentNullException>(() => policy.ShouldBlockAdvance(null!));
    }
}