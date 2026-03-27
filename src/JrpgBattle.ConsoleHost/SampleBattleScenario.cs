// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle.ConsoleHost.Scenario;

public static class SampleBattleScenario
{
    public static BattleDefinition CreateBattleDefinition()
    {
        return new BattleDefinition(
            partyActors: new List<BattleActorDefinition>
            {
                new(
                    id: "hero_1",
                    name: "Hero",
                    team: BattleTeam.Party,
                    maxHp: 20),
                new(
                    id: "hero_2",
                    name: "Mage",
                    team: BattleTeam.Party,
                    maxHp: 15),
            },
            enemyActors: new List<BattleActorDefinition>
            {
                new(
                    id: "slime_1",
                    name: "Slime A",
                    team: BattleTeam.Enemy,
                    maxHp: 10),
                new(
                    id: "slime_2",
                    name: "Slime B",
                    team: BattleTeam.Enemy,
                    maxHp: 10),
            });
    }
}