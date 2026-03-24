// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

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
        // Arrange
        IBattleRuntime runtime = CreateRuntime();

        // Act
        BattleAdvanceResult result = runtime.Advance();

        // Assert
        Assert.IsTrue(result.HasChanged);
        Assert.IsTrue(result.IsPlayerInputNeeded);
        Assert.IsNull(result.ActionResult);
        Assert.IsNull(result.BattleResult);
    }

    [TestMethod]
    public void SubmitPlayerAction_Should_Resolve_Action_And_Not_End_Battle_When_Enemies_Remain()
    {
        // Arrange
        IBattleRuntime runtime = CreateRuntime();

        BattleAdvanceResult advanceResult = runtime.Advance();
        Assert.IsTrue(advanceResult.IsPlayerInputNeeded);

        // Act
        BattleAdvanceResult result = runtime.SubmitPlayerAction(
            new BattleActionChoice(
                actorId: "hero",
                actionKind: BattleActionKind.Attack,
                targetId: "slime_1"));

        // Assert
        Assert.IsTrue(result.HasChanged);
        Assert.IsFalse(result.IsPlayerInputNeeded);
        Assert.IsNotNull(result.ActionResult);
        Assert.IsNull(result.BattleResult);

        BattleRuntimeView view = runtime.GetView();
        BattleCombatantState slime = view.State.Combatants.Single(c => c.Id == "slime_1");
        Assert.AreEqual(5, slime.CurrentHp);
    }

    [TestMethod]
    public void Advance_Should_Request_Player_Input_Again_After_Player_Action_When_Flow_Selects_Player_Again()
    {
        IBattleRuntime runtime = CreateRuntime();

        BattleAdvanceResult advanceResult = runtime.Advance();
        Assert.IsTrue(advanceResult.IsPlayerInputNeeded);

        BattleAdvanceResult playerResult = runtime.SubmitPlayerAction(
            new BattleActionChoice(
                actorId: "hero",
                actionKind: BattleActionKind.Attack,
                targetId: "slime_1"));

        Assert.IsNotNull(playerResult.ActionResult);

        BattleAdvanceResult nextResult = runtime.Advance();

        Assert.IsTrue(nextResult.HasChanged);
        Assert.IsTrue(nextResult.IsPlayerInputNeeded);
        Assert.IsNull(nextResult.ActionResult);
        Assert.IsNull(nextResult.BattleResult);
    }

    [TestMethod]
    public void Battle_Should_End_In_Victory_When_Player_Defeats_Last_Enemy()
    {
        // Arrange
        IBattleRuntime runtime = CreateRuntime();

        // Act
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

        // Assert
        Assert.IsNotNull(step4.BattleResult);
        Assert.AreEqual(BattleOutcome.Victory, step4.BattleResult.Outcome);
        Assert.IsNotNull(step4.BattleResult.Reward);
        Assert.AreEqual(10, step4.BattleResult.Reward!.ExperiencePoints);

        BattleRuntimeView view = runtime.GetView();
        Assert.IsTrue(view.State.IsEnded);
        Assert.AreEqual(BattleOutcome.Victory, view.State.Outcome);
    }

    [TestMethod]
    public void SubmitPlayerAction_Should_Throw_When_Runtime_Is_Not_Waiting_For_Player_Input()
    {
        // Arrange
        IBattleRuntime runtime = CreateRuntime();

        // Act + Assert
        InvalidOperationException exception = Assert.ThrowsException<InvalidOperationException>(
            () => runtime.SubmitPlayerAction(
                new BattleActionChoice("hero", BattleActionKind.Attack, "slime_1")));

        Assert.AreEqual("Battle is not waiting for a player choice.", exception.Message);
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
            new FirstLivingBattleFlow(),
            new AlwaysAttackEnemyActionChooser(),
            new FirstLivingEnemyTargetChooser(),
            new FixedDamageBattleActionResolver(5),
            new FixedXpBattleRewardCalculator(10),
            new AlwaysBlockOnPlayerChoicePolicy());

        return factory.Create(definition);
    }
}