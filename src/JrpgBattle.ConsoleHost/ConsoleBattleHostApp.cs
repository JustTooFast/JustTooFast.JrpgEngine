// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
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
        do
        {
            RunSingleBattle();
        }
        while (PromptBattleAgain());

        return 0;
    }

    private void RunSingleBattle()
    {
        BattleDefinition definition = V0BattleScenario.CreateBattleDefinition();
        IBattleRuntime runtime = _battleRuntimeFactory.Create(definition);

        Console.Clear();
        Console.WriteLine("Battle started.");
        Console.WriteLine();

        WriteBattleState(runtime.GetView().State);

        while (true)
        {
            BattleAdvanceResult advanceResult = runtime.Advance();

            if (advanceResult.ActionResult is not null)
            {
                WriteActionResult(runtime.GetView().State, advanceResult.ActionResult);
            }

            if (advanceResult.BattleResult is not null)
            {
                WriteBattleResult(advanceResult.BattleResult);

                if (advanceResult.BattleResult.Reward is not null)
                {
                    _battleRewardApplier.Apply(advanceResult.BattleResult.Reward);
                }

                return;
            }

            if (advanceResult.IsPlayerInputNeeded)
            {
                BattleActionChoice playerAction = PromptForPlayerAction(runtime.GetView());
                BattleAdvanceResult playerResult = runtime.SubmitPlayerAction(playerAction);

                if (playerResult.ActionResult is not null)
                {
                    WriteActionResult(runtime.GetView().State, playerResult.ActionResult);
                }

                if (playerResult.BattleResult is not null)
                {
                    WriteBattleResult(playerResult.BattleResult);

                    if (playerResult.BattleResult.Reward is not null)
                    {
                        _battleRewardApplier.Apply(playerResult.BattleResult.Reward);
                    }

                    return;
                }
            }
        }
    }

    private static BattleActionChoice PromptForPlayerAction(BattleRuntimeView runtimeView)
    {
        if (runtimeView is null)
        {
            throw new ArgumentNullException(nameof(runtimeView));
        }

        BattleState state = runtimeView.State;

        if (string.IsNullOrWhiteSpace(runtimeView.PendingPlayerActorId))
        {
            throw new InvalidOperationException("No pending player actor is available.");
        }

        BattleCombatantState actor = state.Combatants.FirstOrDefault(c => c.Id == runtimeView.PendingPlayerActorId)
            ?? throw new InvalidOperationException($"Actor '{runtimeView.PendingPlayerActorId}' not found.");

        Console.WriteLine($"It is {actor.Name}'s turn.");
        Console.WriteLine();

        BattleActionKind[] actionKinds = Enum.GetValues<BattleActionKind>();

        for (int i = 0; i < actionKinds.Length; i++)
        {
            Console.WriteLine($"{i + 1}. {actionKinds[i]}");
        }

        Console.WriteLine();

        int actionIndex = PromptForNumber("Choose an action: ", 1, actionKinds.Length);
        BattleActionKind actionKind = actionKinds[actionIndex - 1];

        string? targetId = null;

        if (ActionRequiresTarget(actionKind))
        {
            IReadOnlyList<BattleCombatantState> validTargets = GetValidTargetsForAction(state, actor, actionKind);

            Console.WriteLine();
            Console.WriteLine("Choose a target:");

            for (int i = 0; i < validTargets.Count; i++)
            {
                BattleCombatantState target = validTargets[i];
                Console.WriteLine($"{i + 1}. {target.Name} ({target.CurrentHp}/{target.MaxHp} HP)");
            }

            Console.WriteLine();

            int targetIndex = PromptForNumber("Choose a target: ", 1, validTargets.Count);
            targetId = validTargets[targetIndex - 1].Id;
        }

        Console.WriteLine();

        return new BattleActionChoice(
            actorId: actor.Id,
            actionKind: actionKind,
            targetId: targetId);
    }

    private static bool PromptBattleAgain()
    {
        while (true)
        {
            Console.WriteLine();
            Console.Write("Battle again? (y/n): ");

            string? input = Console.ReadLine()?.Trim();

            if (string.Equals(input, "y", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "yes", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine();
                return true;
            }

            if (string.Equals(input, "n", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "no", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            Console.WriteLine("Please enter y or n.");
        }
    }

    private static bool ActionRequiresTarget(BattleActionKind actionKind)
    {
        return actionKind == BattleActionKind.Attack;
    }

    private static IReadOnlyList<BattleCombatantState> GetValidTargetsForAction(
        BattleState state,
        BattleCombatantState actor,
        BattleActionKind actionKind)
    {
        if (actionKind != BattleActionKind.Attack)
        {
            return Array.Empty<BattleCombatantState>();
        }

        BattleTeam targetTeam = actor.Team == BattleTeam.Party
            ? BattleTeam.Enemy
            : BattleTeam.Party;

        return state.Combatants
            .Where(c => c.Team == targetTeam && c.IsAlive)
            .ToList();
    }

    private static int PromptForNumber(string prompt, int minValue, int maxValue)
    {
        while (true)
        {
            Console.Write(prompt);

            string? input = Console.ReadLine();

            if (int.TryParse(input, out int value) && value >= minValue && value <= maxValue)
            {
                return value;
            }

            Console.WriteLine($"Please enter a number from {minValue} to {maxValue}.");
        }
    }

    private static void WriteBattleState(BattleState state)
    {
        Console.WriteLine("Party:");
        int partyIndex = 1;
        foreach (BattleCombatantState combatant in state.Combatants.Where(c => c.Team == BattleTeam.Party))
        {
            Console.WriteLine($"{partyIndex}. {combatant.Name}: {combatant.CurrentHp}/{combatant.MaxHp} HP");
            partyIndex++;
        }

        Console.WriteLine();

        Console.WriteLine("Enemies:");
        int enemyIndex = 1;
        foreach (BattleCombatantState combatant in state.Combatants.Where(c => c.Team == BattleTeam.Enemy))
        {
            Console.WriteLine($"{enemyIndex}. {combatant.Name}: {combatant.CurrentHp}/{combatant.MaxHp} HP");
            enemyIndex++;
        }

        Console.WriteLine();
    }

    private static void WriteActionResult(BattleState state, BattleActionResult actionResult)
    {
        BattleCombatantState actor = state.Combatants.First(c => c.Id == actionResult.ActorId);

        switch (actionResult.ActionKind)
        {
            case BattleActionKind.Attack:
                {
                    BattleCombatantState target = state.Combatants.First(c => c.Id == actionResult.TargetId);

                    if (actionResult.WasMiss)
                    {
                        Console.WriteLine($"{actor.Name} attacked {target.Name}, but missed.");
                    }
                    else
                    {
                        Console.WriteLine($"{actor.Name} attacked {target.Name} for {actionResult.DamageDealt} damage.");
                    }

                    if (actionResult.TargetDefeated)
                    {
                        Console.WriteLine($"{target.Name} was defeated.");
                    }

                    break;
                }

            case BattleActionKind.Defend:
                Console.WriteLine($"{actor.Name} defended.");
                break;

            case BattleActionKind.Escape:
                if (actionResult.WasEscapeSuccessful)
                {
                    Console.WriteLine($"{actor.Name} escaped successfully.");
                }
                else
                {
                    Console.WriteLine($"{actor.Name} tried to escape, but failed.");
                }

                break;

            case BattleActionKind.Wait:
                Console.WriteLine($"{actor.Name} waited.");
                break;

            case BattleActionKind.Magic:
                Console.WriteLine($"{actor.Name} used Magic.");
                break;

            case BattleActionKind.Item:
                Console.WriteLine($"{actor.Name} used an Item.");
                break;

            case BattleActionKind.Skill:
                Console.WriteLine($"{actor.Name} used a Skill.");
                break;

            default:
                Console.WriteLine($"{actor.Name} acted.");
                break;
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