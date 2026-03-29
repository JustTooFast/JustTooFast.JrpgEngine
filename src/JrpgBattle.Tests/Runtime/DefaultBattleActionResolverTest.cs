// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustTooFast.JrpgBattle.Tests.Runtime;

[TestClass]
public sealed class DefaultBattleActionResolverTests
{
    [TestMethod]
    public void Resolve_Should_Throw_When_Context_Is_Null()
    {
        DefaultBattleActionResolver resolver = new();

        Assert.ThrowsException<ArgumentNullException>(() =>
            resolver.Resolve(null!));
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Actor_Is_Not_Found()
    {
        DefaultBattleActionResolver resolver = new();

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(() =>
            resolver.Resolve(CreateContext(
                CreateDefaultState(),
                "missing",
                new BattleActionChoice(
                    BattleActionKind.Defend,
                    null,
                    BattleTargetMode.None,
                    null))));

        Assert.AreEqual("Actor 'missing' not found.", ex.Message);
    }

    [TestMethod]
    public void Resolve_Should_Throw_When_Actor_Is_Defeated()
    {
        DefaultBattleActionResolver resolver = new();

        BattleState state = new(
        [
            new BattleActorState("hero", "Hero", BattleTeam.Party, 0, 10),
            new BattleActorState("slime", "Slime", BattleTeam.Enemy, 10, 10)
        ]);

        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(() =>
            resolver.Resolve(CreateContext(
                state,
                "hero",
                new BattleActionChoice(
                    BattleActionKind.Defend,
                    null,
                    BattleTargetMode.None,
                    null))));

        Assert.AreEqual("Actor is defeated.", ex.Message);
    }

    [TestMethod]
    public void Resolve_Should_Return_Damage_Operation_For_Valid_Attack_Target()
    {
        DefaultBattleActionResolver resolver = new();

        BattleResolution resolution = resolver.Resolve(CreateContext(
            CreateDefaultState(),
            "hero",
            new BattleActionChoice(
                BattleActionKind.Attack,
                null,
                BattleTargetMode.SingleTarget,
                new[] { "slime" })));

        Assert.AreEqual(1, resolution.Operations.Count);
        Assert.IsInstanceOfType<DamageOperation>(resolution.Operations.Single());

        DamageOperation damage = (DamageOperation)resolution.Operations.Single();
        Assert.AreEqual("slime", damage.TargetId);
        Assert.AreEqual(1, damage.Amount);
    }

    [TestMethod]
    public void Resolve_Should_Return_Empty_Resolution_For_Attack_When_Target_Is_Not_Found()
    {
        DefaultBattleActionResolver resolver = new();

        BattleResolution resolution = resolver.Resolve(CreateContext(
            CreateDefaultState(),
            "hero",
            new BattleActionChoice(
                BattleActionKind.Attack,
                null,
                BattleTargetMode.SingleTarget,
                new[] { "missing" })));

        Assert.AreEqual(0, resolution.Operations.Count);
    }

    [TestMethod]
    public void Resolve_Should_Return_Empty_Resolution_For_Attack_When_Target_Is_On_Same_Team()
    {
        DefaultBattleActionResolver resolver = new();

        BattleResolution resolution = resolver.Resolve(CreateContext(
            CreateDefaultState(),
            "hero",
            new BattleActionChoice(
                BattleActionKind.Attack,
                null,
                BattleTargetMode.SingleTarget,
                new[] { "hero" })));

        Assert.AreEqual(0, resolution.Operations.Count);
    }

    [TestMethod]
    public void Resolve_Should_Return_Empty_Resolution_For_Attack_When_Target_Is_Defeated()
    {
        DefaultBattleActionResolver resolver = new();

        BattleState state = new(
        [
            new BattleActorState("hero", "Hero", BattleTeam.Party, 10, 10),
            new BattleActorState("slime", "Slime", BattleTeam.Enemy, 0, 10)
        ]);

        BattleResolution resolution = resolver.Resolve(CreateContext(
            state,
            "hero",
            new BattleActionChoice(
                BattleActionKind.Attack,
                null,
                BattleTargetMode.SingleTarget,
                new[] { "slime" })));

        Assert.AreEqual(0, resolution.Operations.Count);
    }

    [TestMethod]
    public void Resolve_Should_Return_Defend_Operation_For_Defend()
    {
        DefaultBattleActionResolver resolver = new();

        BattleResolution resolution = resolver.Resolve(CreateContext(
            CreateDefaultState(),
            "hero",
            new BattleActionChoice(
                BattleActionKind.Defend,
                null,
                BattleTargetMode.None,
                null)));

        Assert.AreEqual(1, resolution.Operations.Count);
        Assert.IsInstanceOfType<DefendAppliedOperation>(resolution.Operations.Single());
    }

    [TestMethod]
    public void Resolve_Should_Return_Empty_Resolution_For_Escape_Base_Action()
    {
        DefaultBattleActionResolver resolver = new();

        BattleResolution resolution = resolver.Resolve(CreateContext(
            CreateDefaultState(),
            "hero",
            new BattleActionChoice(
                BattleActionKind.Escape,
                null,
                BattleTargetMode.None,
                null)));

        Assert.AreEqual(0, resolution.Operations.Count);
    }

    [TestMethod]
    public void Resolve_Should_Return_Empty_Resolution_For_Wait()
    {
        DefaultBattleActionResolver resolver = new();

        BattleResolution resolution = resolver.Resolve(CreateContext(
            CreateDefaultState(),
            "hero",
            new BattleActionChoice(
                BattleActionKind.Wait,
                null,
                BattleTargetMode.None,
                null)));

        Assert.AreEqual(0, resolution.Operations.Count);
    }

    private static BattleResolverContext CreateContext(
        BattleState state,
        string actorId,
        BattleActionChoice action)
    {
        return new BattleResolverContext(
            definition: CreateDefinition(),
            state: state,
            actorId: actorId,
            action: action);
    }

    private static BattleDefinition CreateDefinition()
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

    private static BattleState CreateDefaultState()
    {
        return new BattleState(
        [
            new BattleActorState("hero", "Hero", BattleTeam.Party, 10, 10),
            new BattleActorState("slime", "Slime", BattleTeam.Enemy, 10, 10)
        ]);
    }
}