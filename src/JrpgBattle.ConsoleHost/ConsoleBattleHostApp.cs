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

        foreach (BattleActorState actor in view.BattleState.Actors)
        {
            Console.WriteLine(
                $"{actor.Name} [{actor.Team}] HP {actor.CurrentHp}/{actor.MaxHp}" +
                (actor.IsDefeated ? " (Defeated)" : string.Empty));
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
        BattleActorState actor = view.BattleState.Actors.First(c => c.Id == inputRequest.ActorId);

        Console.WriteLine();
        Console.WriteLine($"[{inputRequest.CurrentContext.Kind}] {inputRequest.CurrentContext.Title ?? "Choose"} for {actor.Name}:");

        if (inputRequest.RequiresTargetSelection && inputRequest.TargetSelection is not null)
        {
            IReadOnlyList<BattleSelectableTarget> targets = inputRequest.TargetSelection.SelectableTargets;

            for (int i = 0; i < targets.Count; i++)
            {
                BattleSelectableTarget target = targets[i];
                string status = target.IsEnabled ? string.Empty : " (disabled)";
                Console.WriteLine($"{i + 1}. {target.Label}{status}");
            }

            while (true)
            {
                Console.Write("> ");
                string? input = Console.ReadLine();

                if (int.TryParse(input, out int index) &&
                    index >= 1 &&
                    index <= targets.Count)
                {
                    BattleSelectableTarget selected = targets[index - 1];

                    if (!selected.IsEnabled)
                    {
                        Console.WriteLine("Target is not selectable.");
                        continue;
                    }

                    return new BattleActionChoice(
                        actionKind: inputRequest.CurrentContext.ActionKind!.Value,
                        actionId: inputRequest.CurrentContext.ActionId,
                        targetMode: inputRequest.TargetSelection.TargetMode,
                        targetIds: new[] { selected.ActorId });
                }

                Console.WriteLine("Invalid target.");
            }
        }

        IReadOnlyList<BattleMenuOption> options = inputRequest.Options;

        for (int i = 0; i < options.Count; i++)
        {
            BattleMenuOption option = options[i];
            string status = option.IsEnabled ? string.Empty : $" (disabled: {option.DisabledReason})";
            Console.WriteLine($"{i + 1}. {option.Label}{status}");
        }

        while (true)
        {
            Console.Write("> ");
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int index) &&
                index >= 1 &&
                index <= options.Count)
            {
                BattleMenuOption selected = options[index - 1];

                if (!selected.IsEnabled)
                {
                    Console.WriteLine("Option is disabled.");
                    continue;
                }

                if (selected.Kind == BattleMenuOptionKind.Back)
                {
                    return new BattleActionChoice(
                        actionKind: BattleActionKind.Wait,
                        actionId: null,
                        targetMode: BattleTargetMode.None,
                        targetIds: null);
                }

                return new BattleActionChoice(
                    actionKind: selected.ActionKind ?? throw new InvalidOperationException("Selected action option did not provide an action kind."),
                    actionId: selected.ActionId,
                    targetMode: selected.TargetMode,
                    targetIds: null);
            }

            Console.WriteLine("Invalid choice.");
        }
    }
}