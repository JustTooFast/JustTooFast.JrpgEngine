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

        var actors = definition.PartyActors
            .Concat(definition.EnemyActors)
            .Select(c => new BattleActorState(
                id: c.Id,
                name: c.Name,
                team: c.Team,
                currentHp: c.MaxHp,
                maxHp: c.MaxHp))
            .ToList();

        _runtimeState = new BattleRuntimeState(new BattleState(actors));
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
        _runtimeState.SetOccurrences(Array.Empty<BattleOccurrence>());

        if (_runtimeState.Result is not null)
        {
            return;
        }

        // ---- INPUT HANDLING ----
        if (_runtimeState.CurrentInputRequest is not null)
        {
            if (_runtimeState.PendingPlayerChoice is null)
            {
                return;
            }

            string actorId = _runtimeState.CurrentInputRequest.ActorId;
            BattleActionChoice choice = _runtimeState.PendingPlayerChoice;

            _runtimeState.SetInputRequest(null);
            _runtimeState.SetPendingPlayerChoice(null);

            ExecuteAction(actorId, choice);
            return;
        }

        // ---- FLOW ADVANCE ----
        BattleFlowState flowState = BuildFlowState();

        BattleFlowStep flowStep = _flow.Advance(flowState);

        if (!flowStep.HasAdvanced || string.IsNullOrWhiteSpace(flowStep.ReadyActorId))
        {
            return;
        }

        BattleActorState actor = FindActor(flowStep.ReadyActorId);

        // safety guard only — flow should already enforce this
        if (actor.IsDefeated)
        {
            return;
        }

        if (actor.Team == BattleTeam.Party)
        {
            _runtimeState.SetInputRequest(new BattleInputRequest(actor.Id));
            return;
        }

        BattleActionChoice enemyChoice =
            _enemyActionChooser.ChooseAction(_runtimeState.BattleState, actor.Id);

        ExecuteAction(actor.Id, enemyChoice);
    }

    private BattleFlowState BuildFlowState()
    {
        var actorStates = _runtimeState.ActorStates
            .ToDictionary(
                kvp => kvp.Key,
                kvp =>
                {
                    BattleActorRuntimeState runtime = kvp.Value;
                    BattleActorState actor = FindActor(kvp.Key);

                    return new BattleFlowActorState(
                        actorId: actor.Id,
                        isDefeated: actor.IsDefeated,
                        isDefending: runtime.IsDefending,
                        preventsActing: runtime.PreventedFromActing);
                },
                StringComparer.Ordinal);

        return new BattleFlowState(_runtimeState.BattleState, actorStates);
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
        var occurrences = new List<BattleOccurrence>();

        occurrences.Add(new ActionStartedOccurrence(actorId, choice.ActionKind, choice.ActionId));

        BattleResolution resolution =
            _actionResolver.Resolve(_runtimeState.BattleState, actorId, choice);

        ApplyResolution(actorId, resolution, occurrences);

        occurrences.Add(new ActionResolvedOccurrence(actorId, choice.ActionKind, choice.ActionId));

        FinalizeBattleResultIfNeeded();

        _runtimeState.SetOccurrences(occurrences);
    }

    private void ApplyResolution(
        string actorId,
        BattleResolution resolution,
        List<BattleOccurrence> occurrences)
    {
        foreach (BattleOperation operation in resolution.Operations)
        {
            ApplyOperation(actorId, operation, occurrences);
        }

        if (resolution.Operations.Count == 0)
        {
            occurrences.Add(new ActionHadNoEffectOccurrence(actorId));
        }
    }

    private void ApplyOperation(
        string actorId,
        BattleOperation operation,
        List<BattleOccurrence> occurrences)
    {
        switch (operation)
        {
            case DamageOperation damage:
                ApplyDamage(damage, occurrences);
                break;

            case ApplyConditionOperation apply:
                ApplyCondition(apply, occurrences);
                break;

            case RemoveConditionOperation remove:
                RemoveCondition(remove, occurrences);
                break;

            case EscapeSucceededOperation:
                occurrences.Add(new EscapeSucceededOccurrence(actorId));
                _runtimeState.SetResult(new BattleResult(BattleOutcome.Escaped));
                break;

            case EscapeFailedOperation:
                occurrences.Add(new EscapeFailedOccurrence(actorId));
                break;

            case DefendAppliedOperation defend:
                ApplyDefend(defend, occurrences);
                break;

            default:
                throw new NotSupportedException(
                    $"Battle operation '{operation.GetType().Name}' is not supported by the runtime.");
        }
    }

    private void ApplyDamage(DamageOperation damage, List<BattleOccurrence> occurrences)
    {
        BattleActorState target = FindActor(damage.TargetId);
        BattleActorRuntimeState targetState = _runtimeState.GetActorState(target.Id);

        int previousHp = target.CurrentHp;
        int currentHp = Math.Max(0, previousHp - damage.Amount);

        if (currentHp == previousHp)
        {
            occurrences.Add(new DamageHadNoEffectOccurrence(target.Id));
            return;
        }

        ReplaceActor(new BattleActorState(
            id: target.Id,
            name: target.Name,
            team: target.Team,
            currentHp: currentHp,
            maxHp: target.MaxHp));

        occurrences.Add(new HpChangedOccurrence(target.Id, previousHp, currentHp));

        // ---- DAMAGE-TRIGGERED CONDITION CLEAR ----
        foreach (var condition in targetState.GetActiveConditionsClearedByDamage())
        {
            targetState.RemoveCondition(condition);
            occurrences.Add(new ConditionRemovedOccurrence(target.Id, condition.ConditionId));
        }

        // ---- DEFEAT HANDLING ----
        if (previousHp > 0 && currentHp == 0)
        {
            // clear defending
            targetState.ClearDefending();

            // remove ALL remaining conditions (after damage-clear)
            var remainingConditions = targetState.GetActiveConditions().ToList();

            foreach (var condition in remainingConditions)
            {
                targetState.RemoveCondition(condition);
                occurrences.Add(new ConditionRemovedOccurrence(target.Id, condition.ConditionId));
            }

            occurrences.Add(new ActorDefeatedOccurrence(target.Id));
        }
    }

    private void ApplyCondition(ApplyConditionOperation op, List<BattleOccurrence> occurrences)
    {
        var actorState = _runtimeState.GetActorState(op.ActorId);

        var instance = new ConditionInstance(
            op.ActorId,
            op.ConditionId,
            op.DurationBand,
            op.PreventsActing,
            op.ClearedByDamage,
            op.PersistsAfterBattle);

        actorState.AddCondition(instance);

        occurrences.Add(new ConditionAppliedOccurrence(op.ActorId, op.ConditionId));
    }

    private void RemoveCondition(RemoveConditionOperation op, List<BattleOccurrence> occurrences)
    {
        var actorState = _runtimeState.GetActorState(op.ActorId);

        var toRemove = actorState.Conditions
            .Where(c => !c.IsExpired && c.ConditionId == op.ConditionId)
            .ToList();

        foreach (var condition in toRemove)
        {
            actorState.RemoveCondition(condition);
            occurrences.Add(new ConditionRemovedOccurrence(op.ActorId, op.ConditionId));
        }
    }

    private void ApplyDefend(DefendAppliedOperation defend, List<BattleOccurrence> occurrences)
    {
        var actorState = _runtimeState.GetActorState(defend.ActorId);

        actorState.SetDefending(true);

        occurrences.Add(new DefendAppliedOccurrence(defend.ActorId));
    }

    private void ReplaceActor(BattleActorState updatedActor)
    {
        List<BattleActorState> updatedActors = _runtimeState.BattleState.Actors
            .Select(c => c.Id == updatedActor.Id ? updatedActor : c)
            .ToList();

        _runtimeState.SetBattleState(new BattleState(updatedActors));
    }

    private BattleActorState FindActor(string actorId)
    {
        return _runtimeState.BattleState.Actors.FirstOrDefault(c => c.Id == actorId)
            ?? throw new InvalidOperationException($"Actor '{actorId}' was not found.");
    }

    private void FinalizeBattleResultIfNeeded()
    {
        if (_runtimeState.Result is not null)
        {
            return;
        }

        bool anyPartyRemaining = _runtimeState.BattleState.Actors.Any(c => c.Team == BattleTeam.Party && !c.IsDefeated);
        bool anyEnemyRemaining = _runtimeState.BattleState.Actors.Any(c => c.Team == BattleTeam.Enemy && !c.IsDefeated);

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