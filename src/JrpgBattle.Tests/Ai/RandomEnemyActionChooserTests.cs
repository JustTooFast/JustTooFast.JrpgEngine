// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Ai;

[TestClass]
public sealed class RandomEnemyActionChooserTests
{
    [TestMethod]
    public void ChooseAction_Should_Be_Deterministic_With_Seed()
    {
        var chooser1 = new RandomEnemyActionChooser(123);
        var chooser2 = new RandomEnemyActionChooser(123);

        var state = new BattleState(
        [
            new BattleActorState("slime", "Slime", BattleTeam.Enemy, 10, 10),
            new BattleActorState("hero_1", "Hero 1", BattleTeam.Party, 10, 10),
            new BattleActorState("hero_2", "Hero 2", BattleTeam.Party, 10, 10)
        ]);

        BattleDefinition definition = CreateDefinitionForRandomChooser();

        var results1 = new[]
        {
            chooser1.ChooseAction(new BattleChooserContext(definition, state, "slime")),
            chooser1.ChooseAction(new BattleChooserContext(definition, state, "slime")),
            chooser1.ChooseAction(new BattleChooserContext(definition, state, "slime"))
        };

        var results2 = new[]
        {
            chooser2.ChooseAction(new BattleChooserContext(definition, state, "slime")),
            chooser2.ChooseAction(new BattleChooserContext(definition, state, "slime")),
            chooser2.ChooseAction(new BattleChooserContext(definition, state, "slime"))
        };

        CollectionAssert.AreEqual(results1, results2);
    }

    [TestMethod]
    public void ChooseAction_Should_Fall_Back_To_Defend_When_No_Living_Targets_Exist()
    {
        var chooser = new RandomEnemyActionChooser(123);

        var state = new BattleState(
        [
            new BattleActorState("slime", "Slime", BattleTeam.Enemy, 10, 10),
            new BattleActorState("hero_1", "Hero 1", BattleTeam.Party, 0, 10)
        ]);

        BattleActionChoice choice = chooser.ChooseAction(
            new BattleChooserContext(
                definition: CreateDefinitionForRandomChooser(),
                state: state,
                actorId: "slime"));

        Assert.AreEqual(BattleActionKind.Defend, choice.ActionKind);
        Assert.IsNull(choice.TargetIds);
    }

    private static BattleDefinition CreateDefinitionForRandomChooser()
    {
        return new BattleDefinition(
            partyActors:
            [
                new BattleActorDefinition(
                    id: "hero_1",
                    name: "Hero 1",
                    team: BattleTeam.Party,
                    controlKind: BattleActorControlKind.Player,
                    currentHp: 10,
                    maxHp: 10,
                    allowedActions:
                    [
                        new BattleActionDefinition(BattleActionKind.Attack, BattleExtendedData.Empty),
                        new BattleActionDefinition(BattleActionKind.Defend, BattleExtendedData.Empty),
                        new BattleActionDefinition(BattleActionKind.Escape, BattleExtendedData.Empty),
                        new BattleActionDefinition(BattleActionKind.Wait, BattleExtendedData.Empty),
                    ],
                    spells: Array.Empty<BattleAbilityDefinition>(),
                    skills: Array.Empty<BattleAbilityDefinition>(),
                    automatedBehavior: null,
                    extendedData: BattleExtendedData.Empty),

                new BattleActorDefinition(
                    id: "hero_2",
                    name: "Hero 2",
                    team: BattleTeam.Party,
                    controlKind: BattleActorControlKind.Player,
                    currentHp: 10,
                    maxHp: 10,
                    allowedActions:
                    [
                        new BattleActionDefinition(BattleActionKind.Attack, BattleExtendedData.Empty),
                        new BattleActionDefinition(BattleActionKind.Defend, BattleExtendedData.Empty),
                        new BattleActionDefinition(BattleActionKind.Escape, BattleExtendedData.Empty),
                        new BattleActionDefinition(BattleActionKind.Wait, BattleExtendedData.Empty),
                    ],
                    spells: Array.Empty<BattleAbilityDefinition>(),
                    skills: Array.Empty<BattleAbilityDefinition>(),
                    automatedBehavior: null,
                    extendedData: BattleExtendedData.Empty)
            ],
            enemyActors:
            [
                new BattleActorDefinition(
                    id: "slime",
                    name: "Slime",
                    team: BattleTeam.Enemy,
                    controlKind: BattleActorControlKind.Automated,
                    currentHp: 10,
                    maxHp: 10,
                    allowedActions:
                    [
                        new BattleActionDefinition(BattleActionKind.Attack, BattleExtendedData.Empty),
                        new BattleActionDefinition(BattleActionKind.Defend, BattleExtendedData.Empty),
                        new BattleActionDefinition(BattleActionKind.Wait, BattleExtendedData.Empty),
                    ],
                    spells: Array.Empty<BattleAbilityDefinition>(),
                    skills: Array.Empty<BattleAbilityDefinition>(),
                    automatedBehavior: new BattleActorBehaviorDefinition(
                        aggression: BattleBehaviorBand.Medium,
                        selfPreservation: BattleBehaviorBand.Low,
                        supportiveness: BattleBehaviorBand.Low,
                        opportunism: BattleBehaviorBand.Low,
                        focus: BattleBehaviorBand.Low),
                    extendedData: BattleExtendedData.Empty)
            ],
            configuration: new BattleConfiguration(
                startingTeam: BattleTeam.Party,
                openingAdvantage: BattleOpeningAdvantage.None,
                canEscape: true,
                extendedData: BattleExtendedData.Empty),
            teamDefinitions:
            [
                new BattleTeamDefinition(
                    team: BattleTeam.Party,
                    items: Array.Empty<BattleAbilityDefinition>(),
                    extendedData: BattleExtendedData.Empty),
                new BattleTeamDefinition(
                    team: BattleTeam.Enemy,
                    items: Array.Empty<BattleAbilityDefinition>(),
                    extendedData: BattleExtendedData.Empty),
            ]);
    }
}