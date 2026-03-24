// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System.Linq;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests;

[TestClass]
public sealed class BattleRuntimeDefeatTests
{
    [TestMethod]
    public void Battle_Should_End_In_Defeat_When_All_Party_Combatants_Are_Defeated()
    {
        // Arrange
        var runtime = new BattleRuntime(
            new FixedDamageBattleActionResolver(5),
            new FixedXpBattleRewardCalculator(10));

        var definition = new BattleDefinition(
            partyCombatants:
            [
                new BattleCombatantDefinition("hero", "Hero", BattleTeam.Party, maxHp: 10)
            ],
            enemyCombatants:
            [
                new BattleCombatantDefinition("slime", "Slime", BattleTeam.Enemy, maxHp: 20)
            ]);

        var state = runtime.Initialize(definition);

        // Act
        while (!state.IsEnded)
        {
            var actor = state.Combatants.First(c => c.Team == BattleTeam.Enemy && c.IsAlive);
            var target = state.Combatants.First(c => c.Team == BattleTeam.Party && c.IsAlive);

            var action = new BattleActionChoice(
                actorId: actor.Id,
                actionKind: BattleActionKind.Attack,
                targetId: target.Id);

            state = runtime.ApplyAction(state, action);
        }

        var result = runtime.GetResult(state);

        // Assert
        Assert.IsTrue(state.IsEnded);
        Assert.AreEqual(BattleOutcome.Defeat, state.Outcome);

        Assert.IsNotNull(result);
        Assert.AreEqual(BattleOutcome.Defeat, result.Outcome);
        Assert.IsNull(result.Reward);
    }
}