// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

public sealed class BattleRuntime : IBattleRuntime
{
    private readonly IBattleFlow _flow;
    private readonly IEnemyActionChooser _enemyActionChooser;
    private readonly IBattleActionResolver _actionResolver;
    private readonly BattleRuntimeState _runtimeState;

    public BattleRuntime(
        BattleDefinition definition,
        IBattleFlow flow,
        IEnemyActionChooser enemyActionChooser,
        IBattleActionResolver actionResolver)
    {
        if (definition is null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        _flow = flow ?? throw new ArgumentNullException(nameof(flow));
        _enemyActionChooser = enemyActionChooser ?? throw new ArgumentNullException(nameof(enemyActionChooser));
        _actionResolver = actionResolver ?? throw new ArgumentNullException(nameof(actionResolver));

        var combatants = definition.PartyCombatants
            .Concat(definition.EnemyCombatants)
            .Select(c => new BattleCombatantState(
                id: c.Id,
                name: c.Name,
                team: c.Team,
                currentHp: c.MaxHp,
                maxHp: c.MaxHp))
            .ToList();

        _runtimeState = new BattleRuntimeState(new BattleState(combatants));
    }

    public BattleRuntimeView GetView()
    {
        return new BattleRuntimeView(
            _runtimeState.BattleState,
            _runtimeState.CurrentInputRequest,
            _runtimeState.CurrentOccurrences,
            _runtimeState.Result);
    }

    public void Advance()
    {
        if (_runtimeState.Result is not null)
        {
            _runtimeState.SetOccurrences(Array.Empty<BattleOccurrence>());
            return;
        }

        if (_runtimeState.CurrentInputRequest is not null)
        {
            if (_runtimeState.PendingPlayerChoice is null)
            {
                _runtimeState.SetOccurrences(Array.Empty<BattleOccurrence>());
                return;
            }

            string actorId = _runtimeState.CurrentInputRequest.ActorId;
            BattleActionChoice choice = _runtimeState.PendingPlayerChoice;

            _runtimeState.SetInputRequest(null);
            _runtimeState.SetPendingPlayerChoice(null);

            ExecuteAction(actorId, choice);
            return;
        }

        BattleFlowStep flowStep = _flow.Advance(_runtimeState.BattleState);
        if (!flowStep.HasAdvanced || string.IsNullOrWhiteSpace(flowStep.ReadyActorId))
        {
            _runtimeState.SetOccurrences(Array.Empty<BattleOccurrence>());
            return;
        }

        BattleCombatantState actor = FindCombatant(flowStep.ReadyActorId);
        if (actor.IsDefeated)
        {
            _runtimeState.SetOccurrences(Array.Empty<BattleOccurrence>());
            return;
        }

        if (actor.Team == BattleTeam.Party)
        {
            _runtimeState.SetInputRequest(new BattleInputRequest(actor.Id));
            _runtimeState.SetOccurrences(Array.Empty<BattleOccurrence>());
            return;
        }

        BattleActionChoice enemyChoice = _enemyActionChooser.ChooseAction(_runtimeState.BattleState, actor.Id);
        ExecuteAction(actor.Id, enemyChoice);
    }

    public void SubmitPlayerChoice(BattleActionChoice choice)
    {
        if (choice is null)
        {
            throw new ArgumentNullException(nameof(choice));
        }

        if (_runtimeState.Result is not null)
        {
            throw new InvalidOperationException("Cannot submit a player choice after the battle has completed.");
        }

        if (_runtimeState.CurrentInputRequest is null)
        {
            throw new InvalidOperationException("The runtime is not currently requesting player input.");
        }

        if (_runtimeState.PendingPlayerChoice is not null)
        {
            throw new InvalidOperationException("A player choice has already been submitted and is pending execution.");
        }

        _runtimeState.SetPendingPlayerChoice(choice);
    }

    private void ExecuteAction(string actorId, BattleActionChoice choice)
    {
        var occurrences = new List<BattleOccurrence>
        {
            new ActionStartedOccurrence(actorId, choice.ActionKind, choice.ActionId)
        };

        if (choice.ActionKind == BattleActionKind.Defend)
        {
            occurrences.Add(new DefendAppliedOccurrence(actorId));
        }

        BattleResolution resolution = _actionResolver.Resolve(_runtimeState.BattleState, actorId, choice);
        ApplyResolution(actorId, choice, resolution, occurrences);

        occurrences.Add(new ActionResolvedOccurrence(actorId, choice.ActionKind, choice.ActionId));

        FinalizeBattleResultIfNeeded();
        _runtimeState.SetOccurrences(occurrences);
    }

    private void ApplyResolution(
        string actorId,
        BattleActionChoice choice,
        BattleResolution resolution,
        List<BattleOccurrence> occurrences)
    {
        foreach (BattleOperation operation in resolution.Operations)
        {
            switch (operation)
            {
                case DamageOperation damage:
                    ApplyDamage(damage, occurrences);
                    break;

                case EscapeSucceededOperation:
                    occurrences.Add(new EscapeSucceededOccurrence(actorId));
                    _runtimeState.SetResult(new BattleResult(BattleOutcome.Escaped));
                    break;

                case EscapeFailedOperation:
                    occurrences.Add(new EscapeFailedOccurrence(actorId));
                    break;

                default:
                    throw new NotSupportedException(
                        $"Battle operation '{operation.GetType().Name}' is not supported by the runtime.");
            }
        }
    }

    private void ApplyDamage(DamageOperation damage, List<BattleOccurrence> occurrences)
    {
        BattleCombatantState target = FindCombatant(damage.TargetId);

        int previousHp = target.CurrentHp;
        int currentHp = Math.Max(0, previousHp - damage.Amount);

        if (currentHp == previousHp)
        {
            return;
        }

        ReplaceCombatant(new BattleCombatantState(
            id: target.Id,
            name: target.Name,
            team: target.Team,
            currentHp: currentHp,
            maxHp: target.MaxHp));

        occurrences.Add(new HpChangedOccurrence(target.Id, previousHp, currentHp));

        if (previousHp > 0 && currentHp == 0)
        {
            occurrences.Add(new ActorDefeatedOccurrence(target.Id));
        }
    }

    private void ReplaceCombatant(BattleCombatantState updatedCombatant)
    {
        List<BattleCombatantState> updatedCombatants = _runtimeState.BattleState.Combatants
            .Select(c => c.Id == updatedCombatant.Id ? updatedCombatant : c)
            .ToList();

        _runtimeState.SetBattleState(new BattleState(updatedCombatants));
    }

    private BattleCombatantState FindCombatant(string actorId)
    {
        return _runtimeState.BattleState.Combatants.FirstOrDefault(c => c.Id == actorId)
            ?? throw new InvalidOperationException($"Combatant '{actorId}' was not found.");
    }

    private void FinalizeBattleResultIfNeeded()
    {
        if (_runtimeState.Result is not null)
        {
            return;
        }

        bool anyPartyRemaining = _runtimeState.BattleState.Combatants.Any(c => c.Team == BattleTeam.Party && !c.IsDefeated);
        bool anyEnemyRemaining = _runtimeState.BattleState.Combatants.Any(c => c.Team == BattleTeam.Enemy && !c.IsDefeated);

        if (!anyPartyRemaining)
        {
            _runtimeState.SetResult(new BattleResult(BattleOutcome.Defeat));
            return;
        }

        if (!anyEnemyRemaining)
        {
            _runtimeState.SetResult(new BattleResult(BattleOutcome.Victory));
        }
    }
}