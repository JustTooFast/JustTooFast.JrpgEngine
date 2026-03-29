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
public sealed class EscapeChanceBattleActionDecoratorTests
{
    [TestMethod]
    public void Resolve_Should_Delegate_NonEscape_Actions_To_Inner_Resolver()
    {
        IBattleActionResolver resolver = new EscapeChanceBattleActionDecorator(
            new FixedDamageBattleActionDecorator(
                new DefaultBattleActionResolver(),
                damage: 5),
            seed: 123);

        BattleResolution resolution = resolver.Resolve(
            CreateContext(
                escapeSuccessChance: 0.5,
                actorId: "hero",
                action: new BattleActionChoice(
                    BattleActionKind.Attack,
                    null,
                    BattleTargetMode.SingleTarget,
                    new[] { "slime" })));

        Assert.AreEqual(1, resolution.Operations.Count);
        DamageOperation damage = (DamageOperation)resolution.Operations.Single();
        Assert.AreEqual("slime", damage.TargetId);
        Assert.AreEqual(5, damage.Amount);
    }

    [TestMethod]
    public void Resolve_Should_Always_Fail_Escape_When_Chance_Is_Zero()
    {
        IBattleActionResolver resolver = new EscapeChanceBattleActionDecorator(
            new DefaultBattleActionResolver(),
            seed: 123);

        BattleResolution resolution = resolver.Resolve(
            CreateContext(
                escapeSuccessChance: 0.0,
                actorId: "hero",
                action: new BattleActionChoice(
                    BattleActionKind.Escape,
                    null,
                    BattleTargetMode.None,
                    null)));

        Assert.AreEqual(1, resolution.Operations.Count);
        Assert.IsInstanceOfType<EscapeFailedOperation>(resolution.Operations.Single());
    }

    [TestMethod]
    public void Resolve_Should_Always_Succeed_Escape_When_Chance_Is_One()
    {
        IBattleActionResolver resolver = new EscapeChanceBattleActionDecorator(
            new DefaultBattleActionResolver(),
            seed: 123);

        BattleResolution resolution = resolver.Resolve(
            CreateContext(
                escapeSuccessChance: 1.0,
                actorId: "hero",
                action: new BattleActionChoice(
                    BattleActionKind.Escape,
                    null,
                    BattleTargetMode.None,
                    null)));

        Assert.AreEqual(1, resolution.Operations.Count);
        Assert.IsInstanceOfType<EscapeSucceededOperation>(resolution.Operations.Single());
    }

    [TestMethod]
    public void Resolve_Should_Be_Deterministic_For_Escape_With_Same_Seed()
    {
        IBattleActionResolver resolver1 = new EscapeChanceBattleActionDecorator(
            new DefaultBattleActionResolver(),
            seed: 123);

        IBattleActionResolver resolver2 = new EscapeChanceBattleActionDecorator(
            new DefaultBattleActionResolver(),
            seed: 123);

        Type[] results1 =
        [
            resolver1.Resolve(
                CreateContext(
                    escapeSuccessChance: 0.5,
                    actorId: "hero",
                    action: new BattleActionChoice(
                        BattleActionKind.Escape,
                        null,
                        BattleTargetMode.None,
                        null))).Operations.Single().GetType(),

            resolver1.Resolve(
                CreateContext(
                    escapeSuccessChance: 0.5,
                    actorId: "hero",
                    action: new BattleActionChoice(
                        BattleActionKind.Escape,
                        null,
                        BattleTargetMode.None,
                        null))).Operations.Single().GetType(),

            resolver1.Resolve(
                CreateContext(
                    escapeSuccessChance: 0.5,
                    actorId: "hero",
                    action: new BattleActionChoice(
                        BattleActionKind.Escape,
                        null,
                        BattleTargetMode.None,
                        null))).Operations.Single().GetType()
        ];

        Type[] results2 =
        [
            resolver2.Resolve(
                CreateContext(
                    escapeSuccessChance: 0.5,
                    actorId: "hero",
                    action: new BattleActionChoice(
                        BattleActionKind.Escape,
                        null,
                        BattleTargetMode.None,
                        null))).Operations.Single().GetType(),

            resolver2.Resolve(
                CreateContext(
                    escapeSuccessChance: 0.5,
                    actorId: "hero",
                    action: new BattleActionChoice(
                        BattleActionKind.Escape,
                        null,
                        BattleTargetMode.None,
                        null))).Operations.Single().GetType(),

            resolver2.Resolve(
                CreateContext(
                    escapeSuccessChance: 0.5,
                    actorId: "hero",
                    action: new BattleActionChoice(
                        BattleActionKind.Escape,
                        null,
                        BattleTargetMode.None,
                        null))).Operations.Single().GetType()
        ];

        CollectionAssert.AreEqual(results1, results2);
    }

    private static BattleResolverContext CreateContext(
        double escapeSuccessChance,
        string actorId,
        BattleActionChoice action)
    {
        return new BattleResolverContext(
            definition: CreateDefinition(escapeSuccessChance),
            state: CreateState(),
            actorId: actorId,
            action: action);
    }

    private static BattleDefinition CreateDefinition(double escapeSuccessChance)
    {
        return new BattleDefinition(
            partyActors:
            [
                new BattleActorDefinition(
                    id: "hero",
                    name: "Hero",
                    team: BattleTeam.Party,
                    controlKind: BattleActorControlKind.Player,
                    currentHp: 10,
                    maxHp: 10,
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
                    id: "slime",
                    name: "Slime",
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
    }

    private static BattleState CreateState() => new(
    [
        new BattleActorState("hero", "Hero", BattleTeam.Party, 10, 10),
        new BattleActorState("slime", "Slime", BattleTeam.Enemy, 10, 10)
    ]);
}