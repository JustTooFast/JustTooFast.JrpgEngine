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
        BattleDefinition definition = SampleBattleScenario.CreateBattleDefinition();
        IBattleRuntime runtime = _battleRuntimeFactory.Create(definition);

        while (true)
        {
            runtime.Advance();

            BattleRuntimeView view = runtime.GetView();

            RenderState(view);
            RenderOccurrences(view.Occurrences);

            if (view.Result is not null)
            {
                RenderResult(view.Result);

                if (view.Result.Outcome == BattleOutcome.Victory)
                {
                    _battleRewardApplier.Apply(new BattleReward(10));
                }

                return 0;
            }

            if (view.InputRequest is not null)
            {
                BattleActionChoice choice = PromptForPlayerChoice(view, view.InputRequest);
                runtime.SubmitPlayerChoice(choice);
            }
        }
    }

    private static void RenderState(BattleRuntimeView view)
    {
        Console.WriteLine();
        Console.WriteLine("=== Battle State ===");

        foreach (BattleActorView actor in view.BattleState.Actors)
        {
            Console.WriteLine(
                $"{actor.DisplayName} [{actor.Team}] HP {actor.CurrentHp}/{actor.MaxHp}" +
                (actor.IsDefeated ? " (Defeated)" : string.Empty));

            foreach (BattleExtendedDataEntry entry in actor.ExtendedData.Entries)
            {
                Console.WriteLine($"  {entry.Key}: {entry.Value}");
            }
        }

        Console.WriteLine();
    }

    private static void RenderOccurrences(IReadOnlyList<BattleOccurrence> occurrences)
    {
        foreach (BattleOccurrence occurrence in occurrences)
        {
            switch (occurrence)
            {
                case ActionStartedOccurrence started:
                    Console.WriteLine($"{started.ActorId} started {started.ActionKind}.");
                    break;

                case ActionResolvedOccurrence resolved:
                    Console.WriteLine($"{resolved.ActorId} resolved {resolved.ActionKind}.");
                    break;

                case ActionHadNoEffectOccurrence noEffect:
                    Console.WriteLine($"{noEffect.ActorId}'s action had no effect.");
                    break;

                case HpChangedOccurrence hpChanged:
                    if (hpChanged.Delta < 0)
                    {
                        Console.WriteLine($"{hpChanged.ActorId} took {-hpChanged.Delta} damage.");
                    }
                    else if (hpChanged.Delta > 0)
                    {
                        Console.WriteLine($"{hpChanged.ActorId} recovered {hpChanged.Delta} HP.");
                    }
                    break;

                case DamageHadNoEffectOccurrence damageNoEffect:
                    Console.WriteLine($"{damageNoEffect.ActorId} took no damage.");
                    break;

                case ActorDefeatedOccurrence defeated:
                    Console.WriteLine($"{defeated.ActorId} was defeated.");
                    break;

                case EscapeSucceededOccurrence escaped:
                    Console.WriteLine($"{escaped.ActorId} escaped successfully.");
                    break;

                case EscapeFailedOccurrence escapeFailed:
                    Console.WriteLine($"{escapeFailed.ActorId} failed to escape.");
                    break;

                case DefendAppliedOccurrence defendApplied:
                    Console.WriteLine($"{defendApplied.ActorId} is defending.");
                    break;

                default:
                    Console.WriteLine($"Unhandled occurrence: {occurrence.GetType().Name}");
                    break;
            }
        }
    }

    private static void RenderResult(BattleResult result)
    {
        Console.WriteLine();
        Console.WriteLine($"Battle ended: {result.Outcome}");
    }

    private static BattleActionChoice PromptForPlayerChoice(
        BattleRuntimeView view,
        BattleInputRequest inputRequest)
    {
        BattleActorView actor = view.BattleState.Actors.First(c => c.ActorId == inputRequest.ActorId);

        Console.WriteLine();
        Console.WriteLine($"Choose action for {actor.DisplayName}:");

        IReadOnlyList<BattleInputAction> actions = inputRequest.Actions;

        for (int i = 0; i < actions.Count; i++)
        {
            BattleInputAction action = actions[i];
            string grouping = action.Category is null
                ? action.ActionKind.ToString()
                : $"{action.ActionKind} / {action.Category}";
            string status = action.IsEnabled ? string.Empty : " (disabled)";

            Console.WriteLine($"{i + 1}. [{grouping}] {action.DisplayText}{status}");
        }

        while (true)
        {
            Console.Write("> ");
            string? input = Console.ReadLine();

            if (!int.TryParse(input, out int index) || index < 1 || index > actions.Count)
            {
                Console.WriteLine("Invalid choice.");
                continue;
            }

            BattleInputAction selected = actions[index - 1];

            if (!selected.IsEnabled)
            {
                Console.WriteLine("Action is disabled.");
                continue;
            }

            IReadOnlyList<string>? targetIds = null;

            if (selected.TargetMode == BattleTargetMode.SingleTarget)
            {
                targetIds = PromptForSingleTarget(view, actor.Team);
            }
            else if (selected.TargetMode == BattleTargetMode.AllAllies ||
                     selected.TargetMode == BattleTargetMode.AllEnemies ||
                     selected.TargetMode == BattleTargetMode.None)
            {
                targetIds = null;
            }
            else
            {
                throw new NotSupportedException(
                    $"Target mode '{selected.TargetMode}' is not supported by the console host.");
            }

            return new BattleActionChoice(
                actionId: selected.ActionId,
                targetMode: selected.TargetMode,
                targetIds: targetIds);
        }
    }

    private static IReadOnlyList<string> PromptForSingleTarget(
        BattleRuntimeView view,
        BattleTeam actingTeam)
    {
        BattleTeam targetTeam = actingTeam == BattleTeam.Party
            ? BattleTeam.Enemy
            : BattleTeam.Party;

        BattleActorView[] targets = view.BattleState.Actors
            .Where(a => a.Team == targetTeam)
            .ToArray();

        Console.WriteLine();
        Console.WriteLine("Choose target:");

        for (int i = 0; i < targets.Length; i++)
        {
            BattleActorView target = targets[i];
            string status = target.IsDefeated ? " (defeated)" : string.Empty;
            Console.WriteLine($"{i + 1}. {target.DisplayName}{status}");
        }

        while (true)
        {
            Console.Write("> ");
            string? input = Console.ReadLine();

            if (!int.TryParse(input, out int index) || index < 1 || index > targets.Length)
            {
                Console.WriteLine("Invalid target.");
                continue;
            }

            BattleActorView selected = targets[index - 1];

            if (selected.IsDefeated)
            {
                Console.WriteLine("Target is not selectable.");
                continue;
            }

            return new[] { selected.ActorId };
        }
    }
}