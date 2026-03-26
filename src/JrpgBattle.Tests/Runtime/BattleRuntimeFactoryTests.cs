// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Runtime;

[TestClass]
public sealed class BattleRuntimeFactoryTests
{
    [TestMethod]
    public void Constructor_Should_Throw_When_Flow_Is_Null()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
            new BattleRuntimeFactory(
                flowFactory: null!,
                enemyActionChooserFactory: () => new AlwaysAttackEnemyActionChooser(),
                enemyTargetChooserFactory: () => new FirstLivingEnemyTargetChooser(),
                actionResolverFactory: () => CreateFixedResolver(),
                rewardCalculatorFactory: () => new FixedXpBattleRewardCalculator(10),
                playerChoiceBlockingPolicyFactory: () => new AlwaysBlockOnPlayerChoicePolicy()));
    }

    [TestMethod]
    public void Constructor_Should_Throw_When_EnemyActionChooser_Is_Null()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
            new BattleRuntimeFactory(
                flowFactory: () => new FirstLivingBattleFlow(),
                enemyActionChooserFactory: null!,
                enemyTargetChooserFactory: () => new FirstLivingEnemyTargetChooser(),
                actionResolverFactory: () => CreateFixedResolver(),
                rewardCalculatorFactory: () => new FixedXpBattleRewardCalculator(10),
                playerChoiceBlockingPolicyFactory: () => new AlwaysBlockOnPlayerChoicePolicy()));
    }

    [TestMethod]
    public void Constructor_Should_Throw_When_EnemyTargetChooser_Is_Null()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
            new BattleRuntimeFactory(
                flowFactory: () => new FirstLivingBattleFlow(),
                enemyActionChooserFactory: () => new AlwaysAttackEnemyActionChooser(),
                enemyTargetChooserFactory: null!,
                actionResolverFactory: () => CreateFixedResolver(),
                rewardCalculatorFactory: () => new FixedXpBattleRewardCalculator(10),
                playerChoiceBlockingPolicyFactory: () => new AlwaysBlockOnPlayerChoicePolicy()));
    }

    [TestMethod]
    public void Constructor_Should_Throw_When_ActionResolver_Is_Null()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
            new BattleRuntimeFactory(
                flowFactory: () => new FirstLivingBattleFlow(),
                enemyActionChooserFactory: () => new AlwaysAttackEnemyActionChooser(),
                enemyTargetChooserFactory: () => new FirstLivingEnemyTargetChooser(),
                actionResolverFactory: null!,
                rewardCalculatorFactory: () => new FixedXpBattleRewardCalculator(10),
                playerChoiceBlockingPolicyFactory: () => new AlwaysBlockOnPlayerChoicePolicy()));
    }

    [TestMethod]
    public void Constructor_Should_Throw_When_RewardCalculator_Is_Null()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
            new BattleRuntimeFactory(
                flowFactory: () => new FirstLivingBattleFlow(),
                enemyActionChooserFactory: () => new AlwaysAttackEnemyActionChooser(),
                enemyTargetChooserFactory: () => new FirstLivingEnemyTargetChooser(),
                actionResolverFactory: () => CreateFixedResolver(),
                rewardCalculatorFactory: null!,
                playerChoiceBlockingPolicyFactory: () => new AlwaysBlockOnPlayerChoicePolicy()));
    }

    [TestMethod]
    public void Constructor_Should_Throw_When_PlayerChoiceBlockingPolicy_Is_Null()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
            new BattleRuntimeFactory(
                flowFactory: () => new FirstLivingBattleFlow(),
                enemyActionChooserFactory: () => new AlwaysAttackEnemyActionChooser(),
                enemyTargetChooserFactory: () => new FirstLivingEnemyTargetChooser(),
                actionResolverFactory: () => CreateFixedResolver(),
                rewardCalculatorFactory: () => new FixedXpBattleRewardCalculator(10),
                playerChoiceBlockingPolicyFactory: null!));
    }

    [TestMethod]
    public void Create_Should_Throw_When_Definition_Is_Null()
    {
        IBattleRuntimeFactory factory = CreateFactory();

        Assert.ThrowsException<ArgumentNullException>(() => factory.Create(null!));
    }

    [TestMethod]
    public void Create_Should_Return_Runtime()
    {
        IBattleRuntimeFactory factory = CreateFactory();

        IBattleRuntime runtime = factory.Create(CreateDefinition());

        Assert.IsNotNull(runtime);
    }

    [TestMethod]
    public void Create_Should_Return_New_Runtime_Instance_Each_Time()
    {
        IBattleRuntimeFactory factory = CreateFactory();
        BattleDefinition definition = CreateDefinition();

        IBattleRuntime runtime1 = factory.Create(definition);
        IBattleRuntime runtime2 = factory.Create(definition);

        Assert.AreNotSame(runtime1, runtime2);
    }

    private static IBattleRuntimeFactory CreateFactory()
    {
        return new BattleRuntimeFactory(
            flowFactory: () => new FirstLivingBattleFlow(),
            enemyActionChooserFactory: () => new AlwaysAttackEnemyActionChooser(),
            enemyTargetChooserFactory: () => new FirstLivingEnemyTargetChooser(),
            actionResolverFactory: () => CreateFixedResolver(),
            rewardCalculatorFactory: () => new FixedXpBattleRewardCalculator(10),
            playerChoiceBlockingPolicyFactory: () => new AlwaysBlockOnPlayerChoicePolicy());
    }

    private static BattleDefinition CreateDefinition()
    {
        return new BattleDefinition(
            partyCombatants:
            [
                new BattleCombatantDefinition("hero", "Hero", BattleTeam.Party, 10)
            ],
            enemyCombatants:
            [
                new BattleCombatantDefinition("slime", "Slime", BattleTeam.Enemy, 10)
            ]);
    }

    private static IBattleActionResolver CreateFixedResolver()
    {
        return new FixedDamageBattleActionDecorator(
            new DefaultBattleActionResolver(),
            damage: 5);
    }
}