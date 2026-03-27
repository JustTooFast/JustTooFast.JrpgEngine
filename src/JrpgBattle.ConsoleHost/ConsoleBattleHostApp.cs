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

        foreach (BattleCombatantState combatant in view.BattleState.Combatants)
        {
            Console.WriteLine(
                $"{combatant.Name} [{combatant.Team}] HP {combatant.CurrentHp}/{combatant.MaxHp}" +
                (combatant.IsDefeated ? " (Defeated)" : string.Empty));
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
        BattleCombatantState actor = view.BattleState.Combatants.First(c => c.Id == inputRequest.ActorId);

        Console.WriteLine();
        Console.WriteLine($"Choose action for {actor.Name}:");
        Console.WriteLine("1. Attack");
        Console.WriteLine("2. Defend");
        Console.WriteLine("3. Escape");

        while (true)
        {
            Console.Write("> ");
            string? input = Console.ReadLine();

            switch (input)
            {
                case "1":
                {
                    IReadOnlyList<string> targetIds = PromptForAttackTargets(view, actor.Team);
                    return new BattleActionChoice(
                        actionKind: BattleActionKind.Attack,
                        actionId: null,
                        targetIds: targetIds);
                }

                case "2":
                    return new BattleActionChoice(
                        actionKind: BattleActionKind.Defend,
                        actionId: null,
                        targetIds: null);

                case "3":
                    return new BattleActionChoice(
                        actionKind: BattleActionKind.Escape,
                        actionId: null,
                        targetIds: null);
            }

            Console.WriteLine("Invalid choice.");
        }
    }

    private static IReadOnlyList<string> PromptForAttackTargets(BattleRuntimeView view, BattleTeam actorTeam)
    {
        BattleTeam targetTeam = actorTeam == BattleTeam.Party
            ? BattleTeam.Enemy
            : BattleTeam.Party;

        List<BattleCombatantState> targets = view.BattleState.Combatants
            .Where(c => c.Team == targetTeam && !c.IsDefeated)
            .ToList();

        if (targets.Count == 0)
        {
            return Array.Empty<string>();
        }

        Console.WriteLine("Choose target:");

        for (int i = 0; i < targets.Count; i++)
        {
            BattleCombatantState target = targets[i];
            Console.WriteLine($"{i + 1}. {target.Name} ({target.CurrentHp}/{target.MaxHp})");
        }

        while (true)
        {
            Console.Write("> ");
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int index) &&
                index >= 1 &&
                index <= targets.Count)
            {
                return new[] { targets[index - 1].Id };
            }

            Console.WriteLine("Invalid target.");
        }
    }
}