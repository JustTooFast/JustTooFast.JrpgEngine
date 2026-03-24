// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using JustTooFast.JrpgBattle;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;
using JustTooFast.JrpgBattle.ConsoleHost.Scenario;

namespace JustTooFast.JrpgBattle.ConsoleHost;

public sealed class ConsoleBattleHostApp
{
    private readonly IBattleRuntimeFactory _battleRuntimeFactory;

    public ConsoleBattleHostApp(IBattleRuntimeFactory battleRuntimeFactory)
    {
        _battleRuntimeFactory = battleRuntimeFactory ?? throw new ArgumentNullException(nameof(battleRuntimeFactory));
    }

    public int Run()
    {
        BattleDefinition definition = V0BattleScenario.CreateBattleDefinition();
        IBattleRuntime runtime = _battleRuntimeFactory.Create();

        BattleState state = runtime.Initialize(definition);

        WriteBattleStarted(state);

        while (!state.IsEnded)
        {
            BattleActionChoice action = ChooseNextAction(state);
            state = runtime.ApplyAction(state, action);

            WriteBattleState(state);
        }

        BattleResult result = runtime.GetResult(state);

        WriteBattleResult(result);

        return 0;
    }

    private static BattleActionChoice ChooseNextAction(BattleState state)
    {
        BattleCombatantState actor = state.Combatants
            .First(c => c.IsAlive && c.Team == BattleTeam.Party);

        BattleCombatantState target = state.Combatants
            .First(c => c.IsAlive && c.Team == BattleTeam.Enemy);

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

    private static void WriteBattleResult(BattleResult result)
    {
        Console.WriteLine($"Battle ended: {result.Outcome}");

        if (result.Reward is not null)
        {
            Console.WriteLine($"Reward: {result.Reward.ExperiencePoints} XP");
        }
    }
}