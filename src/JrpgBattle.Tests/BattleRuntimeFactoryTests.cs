using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Runtime;

[TestClass]
public sealed class BattleRuntimeFactoryTests
{
    [TestMethod]
    public void Create_Should_Return_Runtime()
    {
        IBattleRuntimeFactory factory = new BattleRuntimeFactory(
            new FirstLivingBattleFlow(),
            new AlwaysAttackEnemyActionChooser(),
            new FirstLivingEnemyTargetChooser(),
            new FixedDamageBattleActionResolver(5),
            new FixedXpBattleRewardCalculator(10),
            new AlwaysBlockOnPlayerChoicePolicy());

        var definition = new BattleDefinition(
            [new("hero", "Hero", BattleTeam.Party, 10)],
            [new("slime", "Slime", BattleTeam.Enemy, 10)]);

        IBattleRuntime runtime = factory.Create(definition);

        Assert.IsNotNull(runtime);
    }
}