// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Ai;

[TestClass]
public sealed class AlwaysAttackEnemyActionChooserTests
{
    [TestMethod]
    public void ChooseAction_Should_Return_Attack_With_First_Living_Opponent_Target()
    {
        var chooser = new AlwaysAttackEnemyActionChooser();

        var state = new BattleState(
        [
            new BattleActorState("slime_1", "Slime 1", BattleTeam.Enemy, 10, 10),
            new BattleActorState("hero_1", "Hero 1", BattleTeam.Party, 10, 10)
        ]);

        BattleActionChoice choice = chooser.ChooseAction(
            new BattleChooserContext(
                definition: CreateDefinition(),
                state: state,
                actorId: "slime_1"));

        Assert.AreEqual(BattleActionKind.Attack, choice.ActionKind);
        Assert.IsNull(choice.ActionId);
        Assert.IsNotNull(choice.TargetIds);
        Assert.AreEqual(1, choice.TargetIds.Count);
        Assert.AreEqual("hero_1", choice.TargetIds.Single());
    }

    [TestMethod]
    public void ChooseAction_Should_Return_Attack_With_No_TargetIds_When_No_Living_Opponent_Exists()
    {
        var chooser = new AlwaysAttackEnemyActionChooser();

        var state = new BattleState(
        [
            new BattleActorState("slime_1", "Slime 1", BattleTeam.Enemy, 10, 10),
            new BattleActorState("hero_1", "Hero 1", BattleTeam.Party, 0, 10)
        ]);

        BattleActionChoice choice = chooser.ChooseAction(
            new BattleChooserContext(
                definition: CreateDefinition(),
                state: state,
                actorId: "slime_1"));

        Assert.AreEqual(BattleActionKind.Attack, choice.ActionKind);
        Assert.IsNull(choice.TargetIds);
    }

    [TestMethod]
    public void ChooseAction_Should_Throw_When_Context_Is_Null()
    {
        var chooser = new AlwaysAttackEnemyActionChooser();

        Assert.ThrowsException<ArgumentNullException>(() => chooser.ChooseAction(null!));
    }

    [TestMethod]
    public void ChooseAction_Should_Throw_When_Actor_Is_Not_Found()
    {
        var chooser = new AlwaysAttackEnemyActionChooser();

        var state = new BattleState(
        [
            new BattleActorState("slime_1", "Slime 1", BattleTeam.Enemy, 10, 10)
        ]);

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
            () => chooser.ChooseAction(
                new BattleChooserContext(
                    definition: CreateDefinition(),
                    state: state,
                    actorId: "missing")));

        Assert.AreEqual("Actor 'missing' not found.", ex.Message);
    }

    [TestMethod]
    public void ChooseAction_Should_Throw_When_Actor_Is_Defeated()
    {
        var chooser = new AlwaysAttackEnemyActionChooser();

        var state = new BattleState(
        [
            new BattleActorState("slime_1", "Slime 1", BattleTeam.Enemy, 0, 10),
            new BattleActorState("hero_1", "Hero 1", BattleTeam.Party, 10, 10)
        ]);

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
            () => chooser.ChooseAction(
                new BattleChooserContext(
                    definition: CreateDefinition(),
                    state: state,
                    actorId: "slime_1")));

        Assert.AreEqual("Actor is defeated.", ex.Message);
    }

    private static BattleDefinition CreateDefinition()
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
                    extendedData: BattleExtendedData.Empty)
            ],
            enemyActors:
            [
                new BattleActorDefinition(
                    id: "slime_1",
                    name: "Slime 1",
                    team: BattleTeam.Enemy,
                    controlKind: BattleActorControlKind.Automated,
                    currentHp: 10,
                    maxHp: 10,
                    allowedActions:
                    [
                        new BattleActionDefinition(BattleActionKind.Attack, BattleExtendedData.Empty),
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
