// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;
using JustTooFast.JrpgBattle.ConsoleHost.Scenario;

namespace JustTooFast.JrpgBattle.ConsoleHost;

public sealed class ConsoleBattleHostApp
{
    private readonly IBattleRuntimeFactory _battleRuntimeFactory;
    private readonly IBattleRewardApplier _battleRewardApplier;

    public ConsoleBattleHostApp(
        IBattleRuntimeFactory battleRuntimeFactory,
        IBattleRewardApplier battleRewardApplier)
    {
        _battleRuntimeFactory = battleRuntimeFactory ?? throw new ArgumentNullException(nameof(battleRuntimeFactory));
        _battleRewardApplier = battleRewardApplier ?? throw new ArgumentNullException(nameof(battleRewardApplier));
    }

    public int Run()
    {
        BattleDefinition definition = V0BattleScenario.CreateBattleDefinition();
        IBattleRuntime runtime = _battleRuntimeFactory.Create(definition);

        WriteBattleStarted(runtime.GetView().State);

        while (true)
        {
            BattleAdvanceResult advanceResult = runtime.Advance();

            if (advanceResult.ActionResult is not null)
            {
                WriteActionResult(runtime.GetView().State, advanceResult.ActionResult);
            }

            if (advanceResult.BattleResult is not null)
            {
                WriteBattleState(runtime.GetView().State);
                WriteBattleResult(advanceResult.BattleResult);

                if (advanceResult.BattleResult.Reward is not null)
                {
                    _battleRewardApplier.Apply(advanceResult.BattleResult.Reward);
                }

                return 0;
            }

            if (advanceResult.IsPlayerInputNeeded)
            {
                BattleActionChoice playerAction = CreatePlayerAction(runtime.GetView().State);
                BattleAdvanceResult playerResult = runtime.SubmitPlayerAction(playerAction);

                if (playerResult.ActionResult is not null)
                {
                    WriteActionResult(runtime.GetView().State, playerResult.ActionResult);
                }

                if (playerResult.BattleResult is not null)
                {
                    WriteBattleState(runtime.GetView().State);
                    WriteBattleResult(playerResult.BattleResult);

                    if (playerResult.BattleResult.Reward is not null)
                    {
                        _battleRewardApplier.Apply(playerResult.BattleResult.Reward);
                    }

                    return 0;
                }
            }
        }
    }

    private static BattleActionChoice CreatePlayerAction(BattleState state)
    {
        BattleCombatantState actor = state.Combatants
            .First(c => c.Team == BattleTeam.Party && c.IsAlive);

        BattleCombatantState target = state.Combatants
            .First(c => c.Team == BattleTeam.Enemy && c.IsAlive);

        return new BattleActionChoice(
            actorId: actor.Id,
            actionKind: BattleActionKind.Attack,
            targetId: target.Id);
    }

    private static void WriteBattleStarted(BattleState state)
    {
        Console.WriteLine("Battle started.");
        Console.WriteLine();
        WriteBattleState(state);
    }

    private static void WriteBattleState(BattleState state)
    {
        Console.WriteLine("Party:");
        foreach (BattleCombatantState combatant in state.Combatants.Where(c => c.Team == BattleTeam.Party))
        {
            Console.WriteLine($"  {combatant.Name}: {combatant.CurrentHp}/{combatant.MaxHp} HP");
        }

        Console.WriteLine("Enemies:");
        foreach (BattleCombatantState combatant in state.Combatants.Where(c => c.Team == BattleTeam.Enemy))
        {
            Console.WriteLine($"  {combatant.Name}: {combatant.CurrentHp}/{combatant.MaxHp} HP");
        }

        Console.WriteLine();
    }

    private static void WriteActionResult(BattleState state, BattleActionResult actionResult)
    {
        BattleCombatantState actor = state.Combatants.First(c => c.Id == actionResult.ActorId);
        BattleCombatantState target = state.Combatants.First(c => c.Id == actionResult.TargetId);

        Console.WriteLine($"{actor.Name} attacked {target.Name} for {actionResult.DamageDealt} damage.");

        if (actionResult.TargetDefeated)
        {
            Console.WriteLine($"{target.Name} was defeated.");
        }

        Console.WriteLine();
        WriteBattleState(state);
    }

    private static void WriteBattleResult(BattleResult result)
    {
        Console.WriteLine($"Battle ended: {result.Outcome}");

        if (result.Reward is not null)
        {
            Console.WriteLine($"Reward: {result.Reward.ExperiencePoints} XP");
        }
    }
}