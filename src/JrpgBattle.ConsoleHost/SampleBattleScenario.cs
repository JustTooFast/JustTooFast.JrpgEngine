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
                    controlKind: BattleActorControlKind.Player,
                    currentHp: 20,
                    maxHp: 20,
                    allowedActions: new List<BattleActionDefinition>
                    {
                        new(BattleActionKind.Attack, BattleExtendedData.Empty),
                        new(BattleActionKind.Defend, BattleExtendedData.Empty),
                        new(BattleActionKind.Item, BattleExtendedData.Empty),
                        new(BattleActionKind.Escape, BattleExtendedData.Empty),
                        new(BattleActionKind.Wait, BattleExtendedData.Empty),
                    },
                    spells: new List<BattleAbilityDefinition>(),
                    skills: new List<BattleAbilityDefinition>(),
                    automatedBehavior: null,
                    extendedData: BattleExtendedData.Empty),

                new(
                    id: "hero_2",
                    name: "Mage",
                    team: BattleTeam.Party,
                    controlKind: BattleActorControlKind.Player,
                    currentHp: 15,
                    maxHp: 15,
                    allowedActions: new List<BattleActionDefinition>
                    {
                        new(BattleActionKind.Attack, BattleExtendedData.Empty),
                        new(BattleActionKind.Defend, BattleExtendedData.Empty),
                        new(BattleActionKind.Magic, BattleExtendedData.Empty),
                        new(BattleActionKind.Item, BattleExtendedData.Empty),
                        new(BattleActionKind.Escape, BattleExtendedData.Empty),
                        new(BattleActionKind.Wait, BattleExtendedData.Empty),
                    },
                    spells: new List<BattleAbilityDefinition>
                    {
                        new(
                            id: "spell_fire",
                            name: "Fire",
                            targetMode: BattleTargetMode.SingleTarget,
                            extendedData: BattleExtendedData.Empty),
                    },
                    skills: new List<BattleAbilityDefinition>(),
                    automatedBehavior: null,
                    extendedData: BattleExtendedData.Empty),
            },
            enemyActors: new List<BattleActorDefinition>
            {
                new(
                    id: "slime_1",
                    name: "Slime A",
                    team: BattleTeam.Enemy,
                    controlKind: BattleActorControlKind.Automated,
                    currentHp: 10,
                    maxHp: 10,
                    allowedActions: new List<BattleActionDefinition>
                    {
                        new(BattleActionKind.Attack, BattleExtendedData.Empty),
                        new(BattleActionKind.Wait, BattleExtendedData.Empty),
                    },
                    spells: new List<BattleAbilityDefinition>(),
                    skills: new List<BattleAbilityDefinition>(),
                    automatedBehavior: new BattleActorBehaviorDefinition(
                        aggression: BattleBehaviorBand.Medium,
                        selfPreservation: BattleBehaviorBand.Low,
                        supportiveness: BattleBehaviorBand.Low,
                        opportunism: BattleBehaviorBand.Low,
                        focus: BattleBehaviorBand.Low),
                    extendedData: BattleExtendedData.Empty),

                new(
                    id: "slime_2",
                    name: "Slime B",
                    team: BattleTeam.Enemy,
                    controlKind: BattleActorControlKind.Automated,
                    currentHp: 10,
                    maxHp: 10,
                    allowedActions: new List<BattleActionDefinition>
                    {
                        new(BattleActionKind.Attack, BattleExtendedData.Empty),
                        new(BattleActionKind.Wait, BattleExtendedData.Empty),
                    },
                    spells: new List<BattleAbilityDefinition>(),
                    skills: new List<BattleAbilityDefinition>(),
                    automatedBehavior: new BattleActorBehaviorDefinition(
                        aggression: BattleBehaviorBand.Medium,
                        selfPreservation: BattleBehaviorBand.Low,
                        supportiveness: BattleBehaviorBand.Low,
                        opportunism: BattleBehaviorBand.Low,
                        focus: BattleBehaviorBand.Low),
                    extendedData: BattleExtendedData.Empty),
            },
            configuration: new BattleConfiguration(
                startingTeam: BattleTeam.Party,
                openingAdvantage: BattleOpeningAdvantage.None,
                canEscape: true,
                extendedData: new BattleExtendedData(
                    [
                        new BattleExtendedDataEntry(
                            EscapeChanceBattleActionDecorator.EscapeSuccessChanceKey,
                            "0.75")
                    ])),
            teamDefinitions: new List<BattleTeamDefinition>
            {
                new(
                    team: BattleTeam.Party,
                    items: new List<BattleAbilityDefinition>
                    {
                        new(
                            id: "item_potion",
                            name: "Potion",
                            targetMode: BattleTargetMode.SingleTarget,
                            extendedData: BattleExtendedData.Empty),
                    },
                    extendedData: BattleExtendedData.Empty),

                new(
                    team: BattleTeam.Enemy,
                    items: new List<BattleAbilityDefinition>(),
                    extendedData: BattleExtendedData.Empty),
            });
    }
}