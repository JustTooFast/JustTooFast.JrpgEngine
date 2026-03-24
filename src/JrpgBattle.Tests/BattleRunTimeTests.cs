// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System.Linq;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests;

[TestClass]
public sealed class BattleRuntimeTests
{
    [TestMethod]
    public void Battle_Should_End_In_Victory_When_All_Enemies_Defeated()
    {
        // Arrange
        var runtime = new BattleRuntime(
            new FixedDamageBattleActionResolver(),
            new FixedXpBattleRewardCalculator(10));

        var definition = new BattleDefinition(
            partyCombatants:
            [
                new BattleCombatantDefinition("hero", "Hero", BattleTeam.Party, maxHp: 20)
            ],
            enemyCombatants:
            [
                new BattleCombatantDefinition("slime", "Slime", BattleTeam.Enemy, maxHp: 10)
            ]);

        var state = runtime.Initialize(definition);

        // Act
        while (!state.IsEnded)
        {
            var actor = state.Combatants.First(c => c.Team == BattleTeam.Party && c.IsAlive);
            var target = state.Combatants.First(c => c.Team == BattleTeam.Enemy && c.IsAlive);

            var action = new BattleActionChoice(
                actorId: actor.Id,
                actionKind: BattleActionKind.Attack,
                targetId: target.Id);

            state = runtime.ApplyAction(state, action);
        }

        var result = runtime.GetResult(state);

        // Assert
        Assert.IsTrue(state.IsEnded);
        Assert.AreEqual(BattleOutcome.Victory, state.Outcome);

        Assert.IsNotNull(result);
        Assert.AreEqual(BattleOutcome.Victory, result.Outcome);
        Assert.IsNotNull(result.Reward);
        Assert.AreEqual(10, result.Reward!.ExperiencePoints);
    }
}