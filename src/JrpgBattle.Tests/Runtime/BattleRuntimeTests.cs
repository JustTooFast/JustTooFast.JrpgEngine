// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Runtime;

[TestClass]
public sealed class BattleRuntimeTests
{
    [TestMethod]
    public void Advance_Should_Request_Player_Input_When_Party_Actor_Becomes_Ready()
    {
        IBattleRuntime runtime = CreateRuntime();

        BattleAdvanceResult result = runtime.Advance();

        Assert.IsTrue(result.HasChanged);
        Assert.IsTrue(result.IsPlayerInputNeeded);
        Assert.IsNull(result.ActionResult);
        Assert.IsNull(result.BattleResult);
    }

    [TestMethod]
    public void Advance_Should_Return_NoChange_When_Battle_Already_Ended()
    {
        IBattleRuntime runtime = CreateRuntime();

        _ = runtime.Advance();
        _ = runtime.SubmitPlayerAction(
            new BattleActionChoice("hero", BattleActionKind.Attack, "slime_1"));

        _ = runtime.Advance();
        BattleAdvanceResult result = runtime.SubmitPlayerAction(
            new BattleActionChoice("hero", BattleActionKind.Attack, "slime_1"));

        Assert.IsNotNull(result.BattleResult);

        BattleAdvanceResult endedResult = runtime.Advance();

        Assert.IsFalse(endedResult.HasChanged);
        Assert.IsFalse(endedResult.IsPlayerInputNeeded);
        Assert.IsNull(endedResult.ActionResult);
        Assert.IsNull(endedResult.BattleResult);
    }

    [TestMethod]
    public void Advance_Should_Return_PlayerInputNeeded_When_PlayerChoice_Is_Pending_And_BlockingPolicy_Blocks()
    {
        IBattleRuntime runtime = CreateRuntime();

        BattleAdvanceResult first = runtime.Advance();
        Assert.IsTrue(first.IsPlayerInputNeeded);

        BattleAdvanceResult second = runtime.Advance();

        Assert.IsFalse(second.HasChanged);
        Assert.IsTrue(second.IsPlayerInputNeeded);
        Assert.IsNull(second.ActionResult);
        Assert.IsNull(second.BattleResult);
    }

    [TestMethod]
    public void SubmitPlayerAction_Should_Resolve_Action_When_Waiting_For_Player_Input()
    {
        IBattleRuntime runtime = CreateRuntime();

        BattleAdvanceResult first = runtime.Advance();
        Assert.IsTrue(first.IsPlayerInputNeeded);

        BattleAdvanceResult result = runtime.SubmitPlayerAction(
            new BattleActionChoice("hero", BattleActionKind.Attack, "slime_1"));

        Assert.IsTrue(result.HasChanged);
        Assert.IsFalse(result.IsPlayerInputNeeded);
        Assert.IsNotNull(result.ActionResult);
        Assert.IsNull(result.BattleResult);

        BattleCombatantState slime = runtime.GetView().State.Combatants.Single(c => c.Id == "slime_1");
        Assert.AreEqual(5, slime.CurrentHp);
    }

    [TestMethod]
    public void SubmitPlayerAction_Should_Not_End_Battle_When_Action_Is_Defend()
    {
        IBattleRuntime runtime = CreateRuntime();

        _ = runtime.Advance();

        BattleAdvanceResult result = runtime.SubmitPlayerAction(
            new BattleActionChoice("hero", BattleActionKind.Defend, targetId: null));

        Assert.IsNotNull(result.ActionResult);
        Assert.IsNull(result.BattleResult);
        Assert.AreEqual(BattleActionKind.Defend, result.ActionResult.ActionKind);

        BattleCombatantState slime = runtime.GetView().State.Combatants.Single(c => c.Id == "slime_1");
        Assert.AreEqual(10, slime.CurrentHp);
    }

    [TestMethod]
    public void SubmitPlayerAction_Should_End_Battle_With_Escaped_When_Action_Is_Escape()
    {
        IBattleRuntime runtime = CreateRuntime();

        _ = runtime.Advance();

        BattleAdvanceResult result = runtime.SubmitPlayerAction(
            new BattleActionChoice("hero", BattleActionKind.Escape, targetId: null));

        Assert.IsNotNull(result.BattleResult);
        Assert.AreEqual(BattleOutcome.Escaped, result.BattleResult.Outcome);
        Assert.IsNull(result.BattleResult.Reward);

        BattleRuntimeView view = runtime.GetView();
        Assert.IsTrue(view.State.IsEnded);
        Assert.AreEqual(BattleOutcome.Escaped, view.State.Outcome);
    }

    [TestMethod]
    public void SubmitPlayerAction_Should_Throw_When_Action_Is_Null()
    {
        IBattleRuntime runtime = CreateRuntime();

        _ = runtime.Advance();

        Assert.ThrowsException<ArgumentNullException>(() => runtime.SubmitPlayerAction(null!));
    }

    [TestMethod]
    public void SubmitPlayerAction_Should_Throw_When_Battle_Has_Already_Ended()
    {
        IBattleRuntime runtime = CreateRuntime();

        _ = runtime.Advance();
        _ = runtime.SubmitPlayerAction(new BattleActionChoice("hero", BattleActionKind.Attack, "slime_1"));
        _ = runtime.Advance();
        _ = runtime.SubmitPlayerAction(new BattleActionChoice("hero", BattleActionKind.Attack, "slime_1"));

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
            () => runtime.SubmitPlayerAction(
                new BattleActionChoice("hero", BattleActionKind.Attack, "slime_1")));

        Assert.AreEqual("Battle has already ended.", ex.Message);
    }

    [TestMethod]
    public void SubmitPlayerAction_Should_Throw_When_Runtime_Is_Not_Waiting_For_Player_Input()
    {
        IBattleRuntime runtime = CreateRuntime();

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
            () => runtime.SubmitPlayerAction(
                new BattleActionChoice("hero", BattleActionKind.Attack, "slime_1")));

        Assert.AreEqual("Battle is not waiting for a player choice.", ex.Message);
    }

    [TestMethod]
    public void SubmitPlayerAction_Should_Throw_When_ActorId_Does_Not_Match_Pending_Player_Actor()
    {
        IBattleRuntime runtime = CreateTwoHeroRuntime();

        BattleAdvanceResult first = runtime.Advance();
        Assert.IsTrue(first.IsPlayerInputNeeded);

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
            () => runtime.SubmitPlayerAction(
                new BattleActionChoice("hero_2", BattleActionKind.Attack, "slime_1")));

        Assert.AreEqual("Submitted action actor does not match the pending player actor.", ex.Message);
    }

    [TestMethod]
    public void SubmitPlayerAction_Should_Throw_When_Submitted_Action_Is_Not_For_Party_Actor()
    {
        IBattleRuntime runtime = CreateRuntime();

        _ = runtime.Advance();

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
            () => runtime.SubmitPlayerAction(
                new BattleActionChoice("slime_1", BattleActionKind.Attack, "hero")));

        Assert.AreEqual("Submitted action actor does not match the pending player actor.", ex.Message);
    }

    [TestMethod]
    public void Battle_Should_End_In_Victory_When_Player_Defeats_Last_Enemy()
    {
        IBattleRuntime runtime = CreateRuntime();

        BattleAdvanceResult step1 = runtime.Advance();
        Assert.IsTrue(step1.IsPlayerInputNeeded);

        BattleAdvanceResult step2 = runtime.SubmitPlayerAction(
            new BattleActionChoice("hero", BattleActionKind.Attack, "slime_1"));
        Assert.IsNull(step2.BattleResult);

        BattleAdvanceResult step3 = runtime.Advance();
        Assert.IsTrue(step3.IsPlayerInputNeeded);
        Assert.IsNull(step3.ActionResult);
        Assert.IsNull(step3.BattleResult);

        BattleAdvanceResult step4 = runtime.SubmitPlayerAction(
            new BattleActionChoice("hero", BattleActionKind.Attack, "slime_1"));

        Assert.IsNotNull(step4.BattleResult);
        Assert.AreEqual(BattleOutcome.Victory, step4.BattleResult.Outcome);
        Assert.IsNotNull(step4.BattleResult.Reward);
        Assert.AreEqual(10, step4.BattleResult.Reward!.ExperiencePoints);

        BattleRuntimeView view = runtime.GetView();
        Assert.IsTrue(view.State.IsEnded);
        Assert.AreEqual(BattleOutcome.Victory, view.State.Outcome);
    }

    [TestMethod]
    public void Advance_Should_Request_Player_Input_Again_After_Player_Action_When_Flow_Selects_Player_Again()
    {
        IBattleRuntime runtime = CreateRuntime();

        BattleAdvanceResult first = runtime.Advance();
        Assert.IsTrue(first.IsPlayerInputNeeded);

        BattleAdvanceResult playerResult = runtime.SubmitPlayerAction(
            new BattleActionChoice("hero", BattleActionKind.Attack, "slime_1"));

        Assert.IsNotNull(playerResult.ActionResult);

        BattleAdvanceResult next = runtime.Advance();

        Assert.IsTrue(next.HasChanged);
        Assert.IsTrue(next.IsPlayerInputNeeded);
        Assert.IsNull(next.ActionResult);
        Assert.IsNull(next.BattleResult);
    }

    [TestMethod]
    public void Advance_Should_End_Battle_With_Defeat_When_Enemy_Defeats_Last_Party_Actor()
    {
        // Arrange
        IBattleRuntime runtime = CreateEnemyFirstDefeatRuntime();

        // Act
        BattleAdvanceResult result = runtime.Advance();

        // Assert
        Assert.IsTrue(result.HasChanged);
        Assert.IsFalse(result.IsPlayerInputNeeded);
        Assert.IsNotNull(result.ActionResult);
        Assert.IsNotNull(result.BattleResult);

        Assert.AreEqual(BattleActionKind.Attack, result.ActionResult.ActionKind);
        Assert.AreEqual(BattleOutcome.Defeat, result.BattleResult.Outcome);
        Assert.IsNull(result.BattleResult.Reward);

        BattleRuntimeView view = runtime.GetView();
        Assert.IsTrue(view.State.IsEnded);
        Assert.AreEqual(BattleOutcome.Defeat, view.State.Outcome);

        BattleCombatantState hero = view.State.Combatants.Single(c => c.Id == "hero");
        Assert.AreEqual(0, hero.CurrentHp);
    }

    [TestMethod]
    public void Advance_Should_Return_NoChange_After_Defeat()
    {
        // Arrange
        IBattleRuntime runtime = CreateEnemyFirstDefeatRuntime();

        // Act
        BattleAdvanceResult defeatResult = runtime.Advance();
        Assert.IsNotNull(defeatResult.BattleResult);
        Assert.AreEqual(BattleOutcome.Defeat, defeatResult.BattleResult.Outcome);

        BattleAdvanceResult nextResult = runtime.Advance();

        // Assert
        Assert.IsFalse(nextResult.HasChanged);
        Assert.IsFalse(nextResult.IsPlayerInputNeeded);
        Assert.IsNull(nextResult.ActionResult);
        Assert.IsNull(nextResult.BattleResult);
    }

    [TestMethod]
    public void Advance_Should_Return_NoChange_After_Escape()
    {
        // Arrange
        IBattleRuntime runtime = CreateRuntime();

        // Act
        _ = runtime.Advance();

        BattleAdvanceResult escapeResult = runtime.SubmitPlayerAction(
            new BattleActionChoice("hero", BattleActionKind.Escape, targetId: null));

        Assert.IsNotNull(escapeResult.BattleResult);
        Assert.AreEqual(BattleOutcome.Escaped, escapeResult.BattleResult.Outcome);

        BattleAdvanceResult nextResult = runtime.Advance();

        // Assert
        Assert.IsFalse(nextResult.HasChanged);
        Assert.IsFalse(nextResult.IsPlayerInputNeeded);
        Assert.IsNull(nextResult.ActionResult);
        Assert.IsNull(nextResult.BattleResult);
    }

    [TestMethod]
    public void SubmitPlayerAction_Should_Throw_After_Escape()
    {
        // Arrange
        IBattleRuntime runtime = CreateRuntime();

        // Act
        _ = runtime.Advance();

        BattleAdvanceResult escapeResult = runtime.SubmitPlayerAction(
            new BattleActionChoice("hero", BattleActionKind.Escape, targetId: null));

        Assert.IsNotNull(escapeResult.BattleResult);
        Assert.AreEqual(BattleOutcome.Escaped, escapeResult.BattleResult.Outcome);

        // Assert
        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
            () => runtime.SubmitPlayerAction(
                new BattleActionChoice("hero", BattleActionKind.Attack, "slime_1")));

        Assert.AreEqual("Battle has already ended.", ex.Message);
    }

    [TestMethod]
    public void SubmitPlayerAction_Should_Not_Change_Any_Combatant_Hp_When_Action_Is_Defend()
    {
        // Arrange
        IBattleRuntime runtime = CreateRuntime();

        _ = runtime.Advance();

        BattleRuntimeView beforeView = runtime.GetView();
        int heroHpBefore = beforeView.State.Combatants.Single(c => c.Id == "hero").CurrentHp;
        int slimeHpBefore = beforeView.State.Combatants.Single(c => c.Id == "slime_1").CurrentHp;

        // Act
        BattleAdvanceResult result = runtime.SubmitPlayerAction(
            new BattleActionChoice("hero", BattleActionKind.Defend, targetId: null));

        // Assert
        Assert.IsNotNull(result.ActionResult);
        Assert.AreEqual(BattleActionKind.Defend, result.ActionResult.ActionKind);
        Assert.IsNull(result.BattleResult);

        BattleRuntimeView afterView = runtime.GetView();
        int heroHpAfter = afterView.State.Combatants.Single(c => c.Id == "hero").CurrentHp;
        int slimeHpAfter = afterView.State.Combatants.Single(c => c.Id == "slime_1").CurrentHp;

        Assert.AreEqual(heroHpBefore, heroHpAfter);
        Assert.AreEqual(slimeHpBefore, slimeHpAfter);
    }

    [TestMethod]
    public void Advance_Should_Resolve_Enemy_Action_When_Enemy_Actor_Becomes_Ready()
    {
        // Arrange
        IBattleRuntime runtime = CreateEnemyFirstRoundRobinRuntime();

        // Act
        BattleAdvanceResult result = runtime.Advance();

        // Assert
        Assert.IsTrue(result.HasChanged);
        Assert.IsFalse(result.IsPlayerInputNeeded);
        Assert.IsNotNull(result.ActionResult);
        Assert.IsNull(result.BattleResult);

        Assert.AreEqual(BattleActionKind.Attack, result.ActionResult.ActionKind);
        Assert.AreEqual("slime_1", result.ActionResult.ActorId);
        Assert.AreEqual("hero", result.ActionResult.TargetId);
        Assert.AreEqual(5, result.ActionResult.DamageDealt);

        BattleRuntimeView view = runtime.GetView();
        BattleCombatantState hero = view.State.Combatants.Single(c => c.Id == "hero");
        Assert.AreEqual(15, hero.CurrentHp);
    }

    [TestMethod]
    public void SubmitPlayerAction_Should_Not_End_Battle_When_Escape_Fails()
    {
        // Arrange
        IBattleRuntime runtime = CreateRuntimeWithFailedEscape();

        BattleAdvanceResult first = runtime.Advance();
        Assert.IsTrue(first.IsPlayerInputNeeded);

        // Act
        BattleAdvanceResult result = runtime.SubmitPlayerAction(
            new BattleActionChoice("hero", BattleActionKind.Escape, targetId: null));

        // Assert
        Assert.IsNotNull(result.ActionResult);
        Assert.IsFalse(result.ActionResult.WasEscapeSuccessful);
        Assert.IsNull(result.BattleResult);

        BattleRuntimeView view = runtime.GetView();
        Assert.IsFalse(view.State.IsEnded);
        Assert.AreEqual(BattleOutcome.None, view.State.Outcome);
    }

    [TestMethod]
    public void SubmitPlayerAction_Should_End_Battle_When_Escape_Succeeds()
    {
        // Arrange
        IBattleRuntime runtime = CreateRuntimeWithSuccessfulEscape();

        BattleAdvanceResult first = runtime.Advance();
        Assert.IsTrue(first.IsPlayerInputNeeded);

        // Act
        BattleAdvanceResult result = runtime.SubmitPlayerAction(
            new BattleActionChoice("hero", BattleActionKind.Escape, targetId: null));

        // Assert
        Assert.IsNotNull(result.ActionResult);
        Assert.IsTrue(result.ActionResult.WasEscapeSuccessful);
        Assert.IsNotNull(result.BattleResult);
        Assert.AreEqual(BattleOutcome.Escaped, result.BattleResult.Outcome);

        BattleRuntimeView view = runtime.GetView();
        Assert.IsTrue(view.State.IsEnded);
        Assert.AreEqual(BattleOutcome.Escaped, view.State.Outcome);
    }

    [TestMethod]
    public void SubmitPlayerAction_Should_Continue_Battle_After_Failed_Escape()
    {
        // Arrange
        IBattleRuntime runtime = CreateRuntimeWithFailedEscape();

        _ = runtime.Advance();

        // Act
        BattleAdvanceResult escapeResult = runtime.SubmitPlayerAction(
            new BattleActionChoice("hero", BattleActionKind.Escape, targetId: null));

        Assert.IsFalse(escapeResult.ActionResult!.WasEscapeSuccessful);

        BattleAdvanceResult next = runtime.Advance();

        // Assert
        Assert.IsTrue(next.HasChanged);
        Assert.IsTrue(
            next.IsPlayerInputNeeded || next.ActionResult != null,
            "Battle should continue after failed escape.");
    }

    [TestMethod]
    public void SubmitPlayerAction_Should_Not_Change_State_When_Action_Is_Wait()
    {
        // Arrange
        IBattleRuntime runtime = CreateRuntime();

        _ = runtime.Advance();

        BattleRuntimeView beforeView = runtime.GetView();
        int heroHpBefore = beforeView.State.Combatants.Single(c => c.Id == "hero").CurrentHp;
        int slimeHpBefore = beforeView.State.Combatants.Single(c => c.Id == "slime_1").CurrentHp;

        // Act
        BattleAdvanceResult result = runtime.SubmitPlayerAction(
            new BattleActionChoice("hero", BattleActionKind.Wait, targetId: null));

        // Assert
        Assert.IsNotNull(result.ActionResult);
        Assert.AreEqual(BattleActionKind.Wait, result.ActionResult.ActionKind);
        Assert.IsNull(result.BattleResult);

        BattleRuntimeView afterView = runtime.GetView();
        int heroHpAfter = afterView.State.Combatants.Single(c => c.Id == "hero").CurrentHp;
        int slimeHpAfter = afterView.State.Combatants.Single(c => c.Id == "slime_1").CurrentHp;

        Assert.AreEqual(heroHpBefore, heroHpAfter);
        Assert.AreEqual(slimeHpBefore, slimeHpAfter);
    }

    [TestMethod]
    public void Advance_Should_Resolve_Enemy_Defend_Without_Requesting_Target()
    {
        IBattleRuntime runtime = CreateEnemyFirstDefendRuntime();

        BattleAdvanceResult result = runtime.Advance();

        Assert.IsTrue(result.HasChanged);
        Assert.IsFalse(result.IsPlayerInputNeeded);
        Assert.IsNotNull(result.ActionResult);
        Assert.AreEqual(BattleActionKind.Defend, result.ActionResult.ActionKind);
        Assert.IsNull(result.ActionResult.TargetId);
        Assert.IsNull(result.BattleResult);
    }

    private static IBattleRuntime CreateRuntime()
    {
        BattleDefinition definition = new(
            partyCombatants:
            [
                new BattleCombatantDefinition("hero", "Hero", BattleTeam.Party, 20)
            ],
            enemyCombatants:
            [
                new BattleCombatantDefinition("slime_1", "Slime 1", BattleTeam.Enemy, 10)
            ]);

        IBattleRuntimeFactory factory = new BattleRuntimeFactory(
            flowFactory: () => new FirstLivingBattleFlow(),
            enemyActionChooserFactory: () => new AlwaysAttackEnemyActionChooser(),
            enemyTargetChooserFactory: () => new FirstLivingEnemyTargetChooser(),
            actionResolverFactory: () => CreateFixedResolver(),
            rewardCalculatorFactory: () => new FixedXpBattleRewardCalculator(10),
            playerChoiceBlockingPolicyFactory: () => new AlwaysBlockOnPlayerChoicePolicy());

        return factory.Create(definition);
    }

    private static IBattleRuntime CreateTwoHeroRuntime()
    {
        BattleDefinition definition = new(
            partyCombatants:
            [
                new BattleCombatantDefinition("hero_1", "Hero 1", BattleTeam.Party, 20),
                new BattleCombatantDefinition("hero_2", "Hero 2", BattleTeam.Party, 20)
            ],
            enemyCombatants:
            [
                new BattleCombatantDefinition("slime_1", "Slime 1", BattleTeam.Enemy, 10)
            ]);

        IBattleRuntimeFactory factory = new BattleRuntimeFactory(
            flowFactory: () => new FirstLivingBattleFlow(),
            enemyActionChooserFactory: () => new AlwaysAttackEnemyActionChooser(),
            enemyTargetChooserFactory: () => new FirstLivingEnemyTargetChooser(),
            actionResolverFactory: () => CreateFixedResolver(),
            rewardCalculatorFactory: () => new FixedXpBattleRewardCalculator(10),
            playerChoiceBlockingPolicyFactory: () => new AlwaysBlockOnPlayerChoicePolicy());

        return factory.Create(definition);
    }

    private static IBattleRuntime CreateEnemyFirstDefeatRuntime()
    {
        BattleDefinition definition = new(
            [new BattleCombatantDefinition("hero", "Hero", BattleTeam.Party, 5)],
            [new BattleCombatantDefinition("slime_1", "Slime 1", BattleTeam.Enemy, 10)]);

        IBattleRuntimeFactory factory = new BattleRuntimeFactory(
            flowFactory: () => new RoundRobinBattleFlow(BattleTeam.Enemy),
            enemyActionChooserFactory: () => new AlwaysAttackEnemyActionChooser(),
            enemyTargetChooserFactory: () => new FirstLivingEnemyTargetChooser(),
            actionResolverFactory: () => CreateFixedResolver(),
            rewardCalculatorFactory: () => new FixedXpBattleRewardCalculator(10),
            playerChoiceBlockingPolicyFactory: () => new AlwaysBlockOnPlayerChoicePolicy());

        return factory.Create(definition);
    }

    private static IBattleRuntime CreateEnemyFirstRoundRobinRuntime()
    {
        BattleDefinition definition = new(
            [new BattleCombatantDefinition("hero", "Hero", BattleTeam.Party, 20)],
            [new BattleCombatantDefinition("slime_1", "Slime 1", BattleTeam.Enemy, 10)]);

        IBattleRuntimeFactory factory = new BattleRuntimeFactory(
            flowFactory: () => new RoundRobinBattleFlow(BattleTeam.Enemy),
            enemyActionChooserFactory: () => new AlwaysAttackEnemyActionChooser(),
            enemyTargetChooserFactory: () => new FirstLivingEnemyTargetChooser(),
            actionResolverFactory: () => CreateFixedResolver(),
            rewardCalculatorFactory: () => new FixedXpBattleRewardCalculator(10),
            playerChoiceBlockingPolicyFactory: () => new AlwaysBlockOnPlayerChoicePolicy());

        return factory.Create(definition);
    }

    private static IBattleActionResolver CreateFixedResolver()
    {
        return new FixedDamageBattleActionDecorator(
            new DefaultBattleActionResolver(),
            damage: 5);
    }

    private static IBattleRuntime CreateRuntimeWithFailedEscape()
    {
        BattleDefinition definition = new(
            [new BattleCombatantDefinition("hero", "Hero", BattleTeam.Party, 20)],
            [new BattleCombatantDefinition("slime_1", "Slime 1", BattleTeam.Enemy, 10)]);

        IBattleRuntimeFactory factory = new BattleRuntimeFactory(
            flowFactory: () => new FirstLivingBattleFlow(),
            enemyActionChooserFactory: () => new AlwaysAttackEnemyActionChooser(),
            enemyTargetChooserFactory: () => new FirstLivingEnemyTargetChooser(),
            actionResolverFactory: () =>
                new EscapeChanceBattleActionDecorator(
                    new FixedDamageBattleActionDecorator(
                        new DefaultBattleActionResolver(),
                        damage: 5),
                    escapeSuccessChance: 0.0,
                    seed: 123),
            rewardCalculatorFactory: () => new FixedXpBattleRewardCalculator(10),
            playerChoiceBlockingPolicyFactory: () => new AlwaysBlockOnPlayerChoicePolicy());

        return factory.Create(definition);
    }

    private static IBattleRuntime CreateRuntimeWithSuccessfulEscape()
    {
        BattleDefinition definition = new(
            [new BattleCombatantDefinition("hero", "Hero", BattleTeam.Party, 20)],
            [new BattleCombatantDefinition("slime_1", "Slime 1", BattleTeam.Enemy, 10)]);

        IBattleRuntimeFactory factory = new BattleRuntimeFactory(
            flowFactory: () => new FirstLivingBattleFlow(),
            enemyActionChooserFactory: () => new AlwaysAttackEnemyActionChooser(),
            enemyTargetChooserFactory: () => new FirstLivingEnemyTargetChooser(),
            actionResolverFactory: () =>
                new EscapeChanceBattleActionDecorator(
                    new FixedDamageBattleActionDecorator(
                        new DefaultBattleActionResolver(),
                        damage: 5),
                    escapeSuccessChance: 1.0,
                    seed: 123),
            rewardCalculatorFactory: () => new FixedXpBattleRewardCalculator(10),
            playerChoiceBlockingPolicyFactory: () => new AlwaysBlockOnPlayerChoicePolicy());

        return factory.Create(definition);
    }

    private static IBattleRuntime CreateEnemyFirstDefendRuntime()
    {
        BattleDefinition definition = new(
            [new BattleCombatantDefinition("hero", "Hero", BattleTeam.Party, 20)],
            [new BattleCombatantDefinition("slime_1", "Slime 1", BattleTeam.Enemy, 10)]);

        IBattleRuntimeFactory factory = new BattleRuntimeFactory(
            flowFactory: () => new RoundRobinBattleFlow(BattleTeam.Enemy),
            enemyActionChooserFactory: () => new AlwaysDefendEnemyActionChooser(),
            enemyTargetChooserFactory: () => new FirstLivingEnemyTargetChooser(),
            actionResolverFactory: () => CreateFixedResolver(),
            rewardCalculatorFactory: () => new FixedXpBattleRewardCalculator(10),
            playerChoiceBlockingPolicyFactory: () => new AlwaysBlockOnPlayerChoicePolicy());

        return factory.Create(definition);
    }
}