// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;
using JustTooFast.JrpgBattle.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Runtime;

[TestClass]
public sealed class BattleRuntimeTests
{
    [TestMethod]
    public void Advance_Should_Request_Player_Input_When_Party_Actor_Becomes_Ready()
    {
        IBattleRuntime runtime = CreateRuntime(
            new ScriptedBattleFlow(
            [
                new BattleFlowStep(true, "hero")
            ]),
            new AlwaysAttackEnemyActionChooser(),
            CreateFixedResolver());

        runtime.Advance();

        BattleRuntimeView view = runtime.GetView();

        Assert.IsNotNull(view.InputRequest);
        Assert.AreEqual("hero", view.InputRequest.ActorId);
        Assert.AreEqual(BattleMenuContextKind.RootCommand, view.InputRequest.CurrentContext.Kind);
        Assert.AreEqual(0, view.Occurrences.Count);
        Assert.IsNull(view.Result);
    }

    [TestMethod]
    public void Advance_Should_Request_Target_Selection_After_Player_Chooses_Single_Target_Action()
    {
        IBattleRuntime runtime = CreateRuntime(
            new ScriptedBattleFlow(
            [
                new BattleFlowStep(true, "hero")
            ]),
            new AlwaysAttackEnemyActionChooser(),
            CreateFixedResolver());

        runtime.Advance();

        runtime.SubmitPlayerChoice(new BattleActionChoice(
            actionKind: BattleActionKind.Attack,
            actionId: null,
            targetMode: BattleTargetMode.SingleTarget,
            targetIds: null));

        runtime.Advance();

        BattleRuntimeView view = runtime.GetView();

        Assert.IsNotNull(view.InputRequest);
        Assert.AreEqual("hero", view.InputRequest.ActorId);
        Assert.AreEqual(BattleMenuContextKind.TargetSelection, view.InputRequest.CurrentContext.Kind);
        Assert.IsTrue(view.InputRequest.RequiresTargetSelection);
        Assert.IsNotNull(view.InputRequest.TargetSelection);
        Assert.AreEqual(BattleTargetMode.SingleTarget, view.InputRequest.TargetSelection.TargetMode);
        Assert.AreEqual(0, view.Occurrences.Count);
        Assert.IsNull(view.Result);
    }

    [TestMethod]
    public void Advance_Should_Execute_Pending_Player_Choice_After_Target_Selection_Is_Completed()
    {
        IBattleRuntime runtime = CreateRuntime(
            new ScriptedBattleFlow(
            [
                new BattleFlowStep(true, "hero")
            ]),
            new AlwaysAttackEnemyActionChooser(),
            CreateFixedResolver());

        runtime.Advance();

        runtime.SubmitPlayerChoice(new BattleActionChoice(
            actionKind: BattleActionKind.Attack,
            actionId: null,
            targetMode: BattleTargetMode.SingleTarget,
            targetIds: null));

        runtime.Advance();

        runtime.SubmitPlayerChoice(new BattleActionChoice(
            actionKind: BattleActionKind.Attack,
            actionId: null,
            targetMode: BattleTargetMode.SingleTarget,
            targetIds: new[] { "slime_1" }));

        runtime.Advance();
        runtime.Advance();

        BattleRuntimeView view = runtime.GetView();
        BattleActorState slime = view.BattleState.Actors.Single(c => c.Id == "slime_1");

        Assert.AreEqual(5, slime.CurrentHp);
        Assert.IsTrue(view.Occurrences.OfType<ActionStartedOccurrence>().Any());
        Assert.IsTrue(view.Occurrences.OfType<HpChangedOccurrence>().Any());
        Assert.IsTrue(view.Occurrences.OfType<ActionResolvedOccurrence>().Any());
        Assert.IsNull(view.InputRequest);
        Assert.IsNull(view.Result);
    }

    [TestMethod]
    public void Advance_Should_End_Battle_With_Victory_When_Last_Enemy_Is_Defeated()
    {
        IBattleRuntime runtime = CreateRuntime(
            new ScriptedBattleFlow(
            [
                new BattleFlowStep(true, "hero")
            ]),
            new AlwaysAttackEnemyActionChooser(),
            CreateFixedResolver(),
            enemyHp: 5);

        runtime.Advance();

        runtime.SubmitPlayerChoice(new BattleActionChoice(
            actionKind: BattleActionKind.Attack,
            actionId: null,
            targetMode: BattleTargetMode.SingleTarget,
            targetIds: null));

        runtime.Advance();

        runtime.SubmitPlayerChoice(new BattleActionChoice(
            actionKind: BattleActionKind.Attack,
            actionId: null,
            targetMode: BattleTargetMode.SingleTarget,
            targetIds: new[] { "slime_1" }));

        runtime.Advance();
        runtime.Advance();

        BattleRuntimeView view = runtime.GetView();

        Assert.IsNotNull(view.Result);
        Assert.AreEqual(BattleOutcome.Victory, view.Result.Outcome);
        Assert.IsTrue(view.Occurrences.OfType<ActorDefeatedOccurrence>().Any());
    }

    [TestMethod]
    public void Advance_Should_End_Battle_With_Defeat_When_Enemy_Defeats_Last_Party_Actor()
    {
        IBattleRuntime runtime = CreateRuntime(
            new ScriptedBattleFlow(
            [
                new BattleFlowStep(true, "slime_1")
            ]),
            new AlwaysAttackEnemyActionChooser(),
            CreateFixedResolver(),
            heroHp: 5);

        runtime.Advance();
        runtime.Advance();

        BattleRuntimeView view = runtime.GetView();
        BattleActorState hero = view.BattleState.Actors.Single(c => c.Id == "hero");

        Assert.AreEqual(0, hero.CurrentHp);
        Assert.IsNotNull(view.Result);
        Assert.AreEqual(BattleOutcome.Defeat, view.Result.Outcome);
    }

    [TestMethod]
    public void Advance_Should_End_Battle_With_Escaped_When_Escape_Succeeds()
    {
        IBattleRuntime runtime = CreateRuntime(
            new ScriptedBattleFlow(
            [
                new BattleFlowStep(true, "hero")
            ]),
            new AlwaysAttackEnemyActionChooser(),
            new EscapeChanceBattleActionDecorator(
                CreateFixedResolver(),
                seed: 123),
            escapeSuccessChance: 1.0);

        runtime.Advance();

        runtime.SubmitPlayerChoice(new BattleActionChoice(
            actionKind: BattleActionKind.Escape,
            actionId: null,
            targetMode: BattleTargetMode.None,
            targetIds: null));

        runtime.Advance();
        runtime.Advance();

        BattleRuntimeView view = runtime.GetView();

        Assert.IsNotNull(view.Result);
        Assert.AreEqual(BattleOutcome.Escaped, view.Result.Outcome);
        Assert.IsTrue(view.Occurrences.OfType<EscapeSucceededOccurrence>().Any());
    }

    [TestMethod]
    public void Advance_Should_Continue_Battle_When_Escape_Fails()
    {
        IBattleRuntime runtime = CreateRuntime(
            new ScriptedBattleFlow(
            [
                new BattleFlowStep(true, "hero")
            ]),
            new AlwaysAttackEnemyActionChooser(),
            new EscapeChanceBattleActionDecorator(
                CreateFixedResolver(),
                seed: 123),
            escapeSuccessChance: 0.0);

        runtime.Advance();

        runtime.SubmitPlayerChoice(new BattleActionChoice(
            actionKind: BattleActionKind.Escape,
            actionId: null,
            targetMode: BattleTargetMode.None,
            targetIds: null));

        runtime.Advance();
        runtime.Advance();

        BattleRuntimeView view = runtime.GetView();

        Assert.IsNull(view.Result);
        Assert.IsTrue(view.Occurrences.OfType<EscapeFailedOccurrence>().Any());
    }

    [TestMethod]
    public void Advance_Should_Emit_NoEffect_Occurrence_For_Empty_Resolution()
    {
        IBattleRuntime runtime = CreateRuntime(
            new ScriptedBattleFlow(
            [
                new BattleFlowStep(true, "hero")
            ]),
            new AlwaysAttackEnemyActionChooser(),
            new DefaultBattleActionResolver());

        runtime.Advance();

        runtime.SubmitPlayerChoice(new BattleActionChoice(
            actionKind: BattleActionKind.Wait,
            actionId: null,
            targetMode: BattleTargetMode.None,
            targetIds: null));

        runtime.Advance();
        runtime.Advance();

        BattleRuntimeView view = runtime.GetView();

        Assert.IsTrue(view.Occurrences.OfType<ActionHadNoEffectOccurrence>().Any());
    }

    [TestMethod]
    public void Advance_Should_Emit_Defend_Applied_Occurrence_For_Defend()
    {
        IBattleRuntime runtime = CreateRuntime(
            new ScriptedBattleFlow(
            [
                new BattleFlowStep(true, "hero")
            ]),
            new AlwaysAttackEnemyActionChooser(),
            CreateFixedResolver());

        runtime.Advance();

        runtime.SubmitPlayerChoice(new BattleActionChoice(
            actionKind: BattleActionKind.Defend,
            actionId: null,
            targetMode: BattleTargetMode.None,
            targetIds: null));

        runtime.Advance();
        runtime.Advance();

        BattleRuntimeView view = runtime.GetView();

        Assert.IsTrue(view.Occurrences.OfType<DefendAppliedOccurrence>().Any());
        Assert.IsNull(view.Result);
    }

    [TestMethod]
    public void SubmitPlayerChoice_Should_Throw_When_Runtime_Is_Not_Requesting_Input()
    {
        IBattleRuntime runtime = CreateRuntime(
            new ScriptedBattleFlow(Array.Empty<BattleFlowStep>()),
            new AlwaysAttackEnemyActionChooser(),
            CreateFixedResolver());

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
            () => runtime.SubmitPlayerChoice(new BattleActionChoice(
                actionKind: BattleActionKind.Attack,
                actionId: null,
                targetMode: BattleTargetMode.SingleTarget,
                targetIds: new[] { "slime_1" })));

        Assert.AreEqual("The runtime is not currently requesting player input.", ex.Message);
    }

    [TestMethod]
    public void SubmitPlayerChoice_Should_Throw_When_Choice_Is_Null()
    {
        IBattleRuntime runtime = CreateRuntime(
            new ScriptedBattleFlow(
            [
                new BattleFlowStep(true, "hero")
            ]),
            new AlwaysAttackEnemyActionChooser(),
            CreateFixedResolver());

        runtime.Advance();

        Assert.ThrowsException<ArgumentNullException>(() => runtime.SubmitPlayerChoice(null!));
    }

    [TestMethod]
    public void Advance_Should_Do_Nothing_After_Battle_Has_Completed()
    {
        IBattleRuntime runtime = CreateRuntime(
            new ScriptedBattleFlow(
            [
                new BattleFlowStep(true, "hero")
            ]),
            new AlwaysAttackEnemyActionChooser(),
            CreateFixedResolver(),
            enemyHp: 5);

        runtime.Advance();

        runtime.SubmitPlayerChoice(new BattleActionChoice(
            actionKind: BattleActionKind.Attack,
            actionId: null,
            targetMode: BattleTargetMode.SingleTarget,
            targetIds: null));

        runtime.Advance();

        runtime.SubmitPlayerChoice(new BattleActionChoice(
            actionKind: BattleActionKind.Attack,
            actionId: null,
            targetMode: BattleTargetMode.SingleTarget,
            targetIds: new[] { "slime_1" }));

        runtime.Advance();
        runtime.Advance();

        Assert.IsNotNull(runtime.GetView().Result);

        runtime.Advance();

        BattleRuntimeView view = runtime.GetView();
        Assert.AreEqual(0, view.Occurrences.Count);
    }

    private static IBattleRuntime CreateRuntime(
        IBattleFlow flow,
        IEnemyActionChooser enemyActionChooser,
        IBattleActionResolver resolver,
        int heroHp = 20,
        int enemyHp = 10,
        double escapeSuccessChance = 0.75)
    {
        BattleDefinition definition = new(
            partyActors:
            [
                new BattleActorDefinition(
                    id: "hero",
                    name: "Hero",
                    team: BattleTeam.Party,
                    controlKind: BattleActorControlKind.Player,
                    currentHp: heroHp,
                    maxHp: 20,
                    allowedActions:
                    [
                        new BattleActionDefinition(BattleActionKind.Attack, BattleExtendedData.Empty),
                        new BattleActionDefinition(BattleActionKind.Defend, BattleExtendedData.Empty),
                        new BattleActionDefinition(BattleActionKind.Item, BattleExtendedData.Empty),
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
                    currentHp: enemyHp,
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
                extendedData: new BattleExtendedData(
                [
                    new BattleExtendedDataEntry(
                        EscapeChanceBattleActionDecorator.EscapeSuccessChanceKey,
                        escapeSuccessChance.ToString("0.0###############", System.Globalization.CultureInfo.InvariantCulture))
                ])),
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

        return new BattleRuntime(
            definition,
            flow,
            enemyActionChooser,
            resolver);
    }

    private static IBattleActionResolver CreateFixedResolver()
    {
        return new FixedDamageBattleActionDecorator(
            new DefaultBattleActionResolver(),
            damage: 5);
    }
}