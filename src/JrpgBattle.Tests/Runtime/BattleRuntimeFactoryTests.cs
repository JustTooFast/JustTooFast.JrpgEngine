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
    public void Constructor_Should_Throw_When_FlowFactory_Is_Null()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
            new BattleRuntimeFactory(
                flowFactory: null!,
                enemyActionChooserFactory: () => new AlwaysAttackEnemyActionChooser(),
                actionResolverFactory: () => CreateFixedResolver()));
    }

    [TestMethod]
    public void Constructor_Should_Throw_When_EnemyActionChooserFactory_Is_Null()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
            new BattleRuntimeFactory(
                flowFactory: () => new RoundRobinBattleFlow(BattleTeam.Party),
                enemyActionChooserFactory: null!,
                actionResolverFactory: () => CreateFixedResolver()));
    }

    [TestMethod]
    public void Constructor_Should_Throw_When_ActionResolverFactory_Is_Null()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
            new BattleRuntimeFactory(
                flowFactory: () => new RoundRobinBattleFlow(BattleTeam.Party),
                enemyActionChooserFactory: () => new AlwaysAttackEnemyActionChooser(),
                actionResolverFactory: null!));
    }

    [TestMethod]
    public void Create_Should_Throw_When_Definition_Is_Null()
    {
        IBattleRuntimeFactory factory = CreateFactory();

        Assert.ThrowsException<ArgumentNullException>(() => factory.Create(null!));
    }

    [TestMethod]
    public void Create_Should_Throw_When_FlowFactory_Returns_Null()
    {
        var factory = new BattleRuntimeFactory(
            flowFactory: () => null!,
            enemyActionChooserFactory: () => new AlwaysAttackEnemyActionChooser(),
            actionResolverFactory: () => CreateFixedResolver());

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(
            () => factory.Create(CreateDefinition()));

        Assert.AreEqual("Flow factory returned null.", ex.Message);
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
            flowFactory: () => new RoundRobinBattleFlow(BattleTeam.Party),
            enemyActionChooserFactory: () => new AlwaysAttackEnemyActionChooser(),
            actionResolverFactory: () => CreateFixedResolver());
    }

    private static BattleDefinition CreateDefinition()
    {
        return new BattleDefinition(
            partyActors:
            [
                new BattleActorDefinition("hero", "Hero", BattleTeam.Party, 10)
            ],
            enemyActors:
            [
                new BattleActorDefinition("slime", "Slime", BattleTeam.Enemy, 10)
            ]);
    }

    private static IBattleActionResolver CreateFixedResolver()
    {
        return new FixedDamageBattleActionDecorator(
            new DefaultBattleActionResolver(),
            damage: 5);
    }
}