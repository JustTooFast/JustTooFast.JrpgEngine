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
    private readonly IBattleFlow _flow;
    private readonly IEnemyActionChooser _enemyActionChooser;
    private readonly IEnemyTargetChooser _enemyTargetChooser;

    public ConsoleBattleHostApp(
        IBattleRuntimeFactory battleRuntimeFactory,
        IBattleFlow flow,
        IEnemyActionChooser enemyActionChooser,
        IEnemyTargetChooser enemyTargetChooser)
    {
        _battleRuntimeFactory = battleRuntimeFactory ?? throw new ArgumentNullException(nameof(battleRuntimeFactory));
        _flow = flow ?? throw new ArgumentNullException(nameof(flow));
        _enemyActionChooser = enemyActionChooser ?? throw new ArgumentNullException(nameof(enemyActionChooser));
        _enemyTargetChooser = enemyTargetChooser ?? throw new ArgumentNullException(nameof(enemyTargetChooser));
    }

    public int Run()
    {
        var definition = V0BattleScenario.CreateBattleDefinition();
        var runtime = _battleRuntimeFactory.Create();

        var state = runtime.Initialize(definition);

        WriteBattleStarted(state);

        while (!state.IsEnded)
        {
            var actorId = _flow.GetNextActorId(state);

            var actor = state.Combatants.FirstOrDefault(c => c.Id == actorId)
                ?? throw new InvalidOperationException($"Actor '{actorId}' not found.");

            BattleActionChoice action;

            if (actor.Team == BattleTeam.Enemy)
            {
                var actionKind = _enemyActionChooser.ChooseAction(state, actorId);
                var targetId = _enemyTargetChooser.ChooseTargetId(state, actorId);

                action = new BattleActionChoice(actorId, actionKind, targetId);
            }
            else
            {
                // v0 simplification: player always attacks first enemy
                var targetId = _enemyTargetChooser.ChooseTargetId(state, actorId);

                action = new BattleActionChoice(actorId, BattleActionKind.Attack, targetId);
            }

            state = runtime.ApplyAction(state, action);

            WriteBattleState(state);
        }

        var result = runtime.GetResult(state);

        WriteBattleResult(result);

        return 0;
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
        foreach (var combatant in state.Combatants.Where(c => c.Team == BattleTeam.Party))
        {
            Console.WriteLine($"  {combatant.Name}: {combatant.CurrentHp}/{combatant.MaxHp} HP");
        }

        Console.WriteLine("Enemies:");
        foreach (var combatant in state.Combatants.Where(c => c.Team == BattleTeam.Enemy))
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