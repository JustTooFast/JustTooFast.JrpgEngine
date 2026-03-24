// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using JustTooFast.JrpgBattle.Abstractions.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Runtime;

[TestClass]
public sealed class BattleRuntimeFactoryTests
{
    [TestMethod]
    public void Create_Should_Return_BattleRuntime()
    {
        // Arrange
        IBattleRuntimeFactory factory = new BattleRuntimeFactory(
            new FixedDamageBattleActionResolver(5),
            new FixedXpBattleRewardCalculator(10));

        // Act
        IBattleRuntime runtime = factory.Create();

        // Assert
        Assert.IsNotNull(runtime);
        Assert.IsInstanceOfType<BattleRuntime>(runtime);
    }

    [TestMethod]
    public void Create_Should_Return_A_New_Runtime_Instance_Each_Time()
    {
        // Arrange
        IBattleRuntimeFactory factory = new BattleRuntimeFactory(
            new FixedDamageBattleActionResolver(5),
            new FixedXpBattleRewardCalculator(10));

        // Act
        IBattleRuntime runtime1 = factory.Create();
        IBattleRuntime runtime2 = factory.Create();

        // Assert
        Assert.AreNotSame(runtime1, runtime2);
    }
}