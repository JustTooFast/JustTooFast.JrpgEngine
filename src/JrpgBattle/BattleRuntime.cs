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
    private readonly IReadOnlyDictionary<string, BattleActorDefinition> _actorDefinitions;

    private PendingExecution? _pendingExecution;
    private PendingTargetSelection? _pendingTargetSelection;

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

        List<BattleActorDefinition> actorDefinitions = definition.PartyActors
            .Concat(definition.EnemyActors)
            .ToList();

        _actorDefinitions = actorDefinitions.ToDictionary(
            static actor => actor.Id,
            StringComparer.Ordinal);

        var actors = actorDefinitions
            .Select(c => new BattleActorState(
                id: c.Id,
                name: c.Name,
                team: c.Team,
                currentHp: c.CurrentHp,
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
        _runtimeState.ClearOccurrences();

        if (_runtimeState.Result is not null)
        {
            return;
        }

        if (_pendingExecution is not null)
        {
            ApplyPendingExecution();
            return;
        }

        if (_runtimeState.CurrentInputRequest is not null)
        {
            if (_runtimeState.PendingPlayerChoice is null)
            {
                return;
            }

            string actorId = _runtimeState.CurrentInputRequest.ActorId;
            BattleActionChoice submittedChoice = _runtimeState.PendingPlayerChoice;

            ValidateSubmittedChoice(actorId, submittedChoice);

            if (_pendingTargetSelection is null)
            {
                HandleCommandSelection(actorId, submittedChoice);
                return;
            }

            HandleTargetSelection(actorId, submittedChoice);
            return;
        }

        BattleFlowState flowState = BuildFlowState();
        BattleFlowStep flowStep = _flow.Advance(flowState);

        FinalizeBattleResultIfNeeded();

        if (_runtimeState.Result is not null)
        {
            return;
        }

        if (!flowStep.HasAdvanced || string.IsNullOrWhiteSpace(flowStep.ReadyActorId))
        {
            return;
        }

        BattleActorState actor = FindActor(flowStep.ReadyActorId);
        BattleActorDefinition actorDefinition = FindActorDefinition(actor.Id);

        if (actor.IsDefeated)
        {
            return;
        }

        if (actorDefinition.ControlKind == BattleActorControlKind.Player)
        {
            _runtimeState.SetInputRequest(CreateRootMenu(actor.Id));
            return;
        }

        BattleActionChoice automatedChoice =
            _enemyActionChooser.ChooseAction(_runtimeState.BattleState, actor.Id);

        ValidateChosenEnemyAction(actor.Id, automatedChoice);

        StagePendingExecution(actor.Id, automatedChoice);
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

        if (_pendingExecution is not null)
        {
            throw new InvalidOperationException("The runtime already has a pending execution.");
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

    private BattleInputRequest CreateRootMenu(string actorId)
    {
        var context = new BattleMenuContext(
            contextId: "root",
            kind: BattleMenuContextKind.RootCommand,
            title: "Command");

        var options = new List<BattleMenuOption>
        {
            new BattleMenuOption(
                optionId: "attack",
                label: "Attack",
                kind: BattleMenuOptionKind.Action,
                isEnabled: true,
                actionKind: BattleActionKind.Attack,
                actionId: null,
                targetMode: BattleTargetMode.SingleTarget),

            new BattleMenuOption(
                optionId: "defend",
                label: "Defend",
                kind: BattleMenuOptionKind.Action,
                isEnabled: true,
                actionKind: BattleActionKind.Defend,
                actionId: null,
                targetMode: BattleTargetMode.None),

            new BattleMenuOption(
                optionId: "escape",
                label: "Escape",
                kind: BattleMenuOptionKind.Action,
                isEnabled: true,
                actionKind: BattleActionKind.Escape,
                actionId: null,
                targetMode: BattleTargetMode.None),

            new BattleMenuOption(
                optionId: "wait",
                label: "Wait",
                kind: BattleMenuOptionKind.Action,
                isEnabled: true,
                actionKind: BattleActionKind.Wait,
                actionId: null,
                targetMode: BattleTargetMode.None)
        };

        return new BattleInputRequest(
            actorId: actorId,
            currentContext: context,
            menuPath: new[] { context },
            options: options,
            targetSelection: null);
    }

    private BattleInputRequest CreateTargetSelectionMenu(
        string actorId,
        BattleActionChoice actionChoice)
    {
        var rootContext = new BattleMenuContext(
            contextId: "root",
            kind: BattleMenuContextKind.RootCommand,
            title: "Command");

        var targetContext = new BattleMenuContext(
            contextId: "target-selection",
            kind: BattleMenuContextKind.TargetSelection,
            title: "Target",
            parentContextId: rootContext.ContextId,
            actionKind: actionChoice.ActionKind,
            actionId: actionChoice.ActionId);

        var selectableTargets = BuildSelectableTargets(actionChoice);

        var targetSelection = new BattleTargetSelectionState(
            targetMode: actionChoice.TargetMode,
            selectableTargets: selectableTargets);

        var options = new List<BattleMenuOption>
        {
            new BattleMenuOption(
                optionId: "back",
                label: "Back",
                kind: BattleMenuOptionKind.Back,
                isEnabled: true,
                actionKind: null,
                actionId: null,
                nextContextId: rootContext.ContextId,
                targetMode: BattleTargetMode.None)
        };

        return new BattleInputRequest(
            actorId: actorId,
            currentContext: targetContext,
            menuPath: new[] { rootContext, targetContext },
            options: options,
            targetSelection: targetSelection);
    }

    private IReadOnlyList<BattleSelectableTarget> BuildSelectableTargets(BattleActionChoice actionChoice)
    {
        IEnumerable<BattleActorState> targets = actionChoice.TargetMode switch
        {
            BattleTargetMode.SingleTarget when actionChoice.ActionKind == BattleActionKind.Attack =>
                _runtimeState.BattleState.Actors.Where(a => a.Team == BattleTeam.Enemy),

            BattleTargetMode.SingleTarget =>
                _runtimeState.BattleState.Actors.Where(a => a.Team == BattleTeam.Enemy),

            _ => throw new InvalidOperationException(
                $"Target selection is not supported for target mode '{actionChoice.TargetMode}'.")
        };

        return targets
            .Select(a => new BattleSelectableTarget(
                actorId: a.Id,
                label: a.Name,
                isEnabled: !a.IsDefeated))
            .ToList();
    }

    private void HandleCommandSelection(string actorId, BattleActionChoice choice)
    {
        ValidateChoiceAgainstMenu(choice);

        _runtimeState.SetPendingPlayerChoice(null);

        if (choice.TargetMode == BattleTargetMode.None)
        {
            _runtimeState.SetInputRequest(null);
            StagePendingExecution(actorId, choice);
            return;
        }

        if (choice.TargetMode == BattleTargetMode.SingleTarget)
        {
            _pendingTargetSelection = new PendingTargetSelection(actorId, choice);
            _runtimeState.SetInputRequest(CreateTargetSelectionMenu(actorId, choice));
            return;
        }

        throw new NotSupportedException(
            $"Player target selection is not yet supported for target mode '{choice.TargetMode}'.");
    }

    private void HandleTargetSelection(string actorId, BattleActionChoice submittedChoice)
    {
        PendingTargetSelection pendingTargetSelection = _pendingTargetSelection
            ?? throw new InvalidOperationException("No pending target selection exists.");

        if (!string.Equals(pendingTargetSelection.ActorId, actorId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Submitted target selection actor does not match the pending actor.");
        }

        ValidateTargetSelectionChoice(submittedChoice, pendingTargetSelection);

        BattleActionChoice finalizedChoice = new BattleActionChoice(
            actionKind: pendingTargetSelection.ActionKind,
            actionId: pendingTargetSelection.ActionId,
            targetMode: pendingTargetSelection.TargetMode,
            targetIds: submittedChoice.TargetIds);

        _pendingTargetSelection = null;
        _runtimeState.SetPendingPlayerChoice(null);
        _runtimeState.SetInputRequest(null);

        StagePendingExecution(actorId, finalizedChoice);
    }

    private void ValidateChoiceAgainstMenu(BattleActionChoice choice)
    {
        BattleInputRequest request = _runtimeState.CurrentInputRequest
            ?? throw new InvalidOperationException("No input request is active.");

        BattleMenuOption? matchingOption = request.Options.FirstOrDefault(o =>
            o.ActionKind == choice.ActionKind &&
            string.Equals(o.ActionId, choice.ActionId, StringComparison.Ordinal));

        if (matchingOption is null)
        {
            throw new InvalidOperationException("Submitted action does not exist in the current menu.");
        }

        if (!matchingOption.IsEnabled)
        {
            throw new InvalidOperationException("Submitted action is currently disabled.");
        }

        if (matchingOption.TargetMode != choice.TargetMode)
        {
            throw new InvalidOperationException("Submitted action target mode does not match menu definition.");
        }
    }

    private void ValidateTargetSelectionChoice(
        BattleActionChoice submittedChoice,
        PendingTargetSelection pendingTargetSelection)
    {
        BattleInputRequest request = _runtimeState.CurrentInputRequest
            ?? throw new InvalidOperationException("No input request is active.");

        if (!request.RequiresTargetSelection || request.TargetSelection is null)
        {
            throw new InvalidOperationException("The current input request is not a target-selection request.");
        }

        if (submittedChoice.ActionKind != pendingTargetSelection.ActionKind)
        {
            throw new InvalidOperationException("Submitted target selection action kind does not match the pending action.");
        }

        if (!string.Equals(submittedChoice.ActionId, pendingTargetSelection.ActionId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Submitted target selection action id does not match the pending action.");
        }

        if (submittedChoice.TargetMode != pendingTargetSelection.TargetMode)
        {
            throw new InvalidOperationException("Submitted target selection target mode does not match the pending action.");
        }

        if (submittedChoice.TargetIds is null || submittedChoice.TargetIds.Count != 1)
        {
            throw new InvalidOperationException("Submitted target selection must include exactly one target id.");
        }

        string targetId = submittedChoice.TargetIds[0];

        BattleSelectableTarget? selectableTarget = request.TargetSelection.SelectableTargets
            .FirstOrDefault(t => string.Equals(t.ActorId, targetId, StringComparison.Ordinal));

        if (selectableTarget is null)
        {
            throw new InvalidOperationException("Submitted target does not exist in the current target-selection request.");
        }

        if (!selectableTarget.IsEnabled)
        {
            throw new InvalidOperationException("Submitted target is currently disabled.");
        }
    }

    private void StagePendingExecution(string actorId, BattleActionChoice choice)
    {
        _pendingExecution = new PendingExecution(actorId, choice);
    }

    private void ApplyPendingExecution()
    {
        PendingExecution pending = _pendingExecution
            ?? throw new InvalidOperationException("No pending execution exists.");

        _pendingExecution = null;

        ExecuteAction(pending.ActorId, pending.Choice);
    }

    private void ValidateSubmittedChoice(string actorId, BattleActionChoice choice)
    {
        BattleActorState actor = FindActor(actorId);

        if (actor.IsDefeated)
        {
            throw new InvalidOperationException($"Actor '{actorId}' cannot submit a choice because they are defeated.");
        }

        if (_runtimeState.CurrentInputRequest is null)
        {
            throw new InvalidOperationException("No input request is active.");
        }

        if (!string.Equals(_runtimeState.CurrentInputRequest.ActorId, actorId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Submitted choice actor does not match the active input request.");
        }
    }

    private void ValidateChosenEnemyAction(string actorId, BattleActionChoice choice)
    {
        if (choice is null)
        {
            throw new InvalidOperationException($"Enemy chooser returned no action for actor '{actorId}'.");
        }

        BattleActorState actor = FindActor(actorId);

        if (actor.IsDefeated)
        {
            throw new InvalidOperationException($"Actor '{actorId}' cannot choose an action because they are defeated.");
        }
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

            if (_runtimeState.Result is not null)
            {
                break;
            }
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

        foreach (ConditionInstance condition in targetState.GetActiveConditionsClearedByDamage())
        {
            targetState.RemoveCondition(condition);
            occurrences.Add(new ConditionRemovedOccurrence(target.Id, condition.ConditionId));
        }

        if (previousHp > 0 && currentHp == 0)
        {
            targetState.ClearDefending();

            List<ConditionInstance> remainingConditions = targetState.GetActiveConditions().ToList();

            foreach (ConditionInstance condition in remainingConditions)
            {
                targetState.RemoveCondition(condition);
                occurrences.Add(new ConditionRemovedOccurrence(target.Id, condition.ConditionId));
            }

            occurrences.Add(new ActorDefeatedOccurrence(target.Id));
        }
    }

    private void ApplyCondition(ApplyConditionOperation op, List<BattleOccurrence> occurrences)
    {
        BattleActorRuntimeState actorState = _runtimeState.GetActorState(op.ActorId);

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
        BattleActorRuntimeState actorState = _runtimeState.GetActorState(op.ActorId);

        List<ConditionInstance> toRemove = actorState.Conditions
            .Where(c => !c.IsExpired && c.ConditionId == op.ConditionId)
            .ToList();

        foreach (ConditionInstance condition in toRemove)
        {
            actorState.RemoveCondition(condition);
            occurrences.Add(new ConditionRemovedOccurrence(op.ActorId, op.ConditionId));
        }
    }

    private void ApplyDefend(DefendAppliedOperation defend, List<BattleOccurrence> occurrences)
    {
        BattleActorRuntimeState actorState = _runtimeState.GetActorState(defend.ActorId);

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

    private BattleActorDefinition FindActorDefinition(string actorId)
    {
        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        }

        if (!_actorDefinitions.TryGetValue(actorId, out BattleActorDefinition? actorDefinition))
        {
            throw new InvalidOperationException($"Actor definition '{actorId}' was not found.");
        }

        return actorDefinition;
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

    private sealed class PendingExecution
    {
        public PendingExecution(string actorId, BattleActionChoice choice)
        {
            ActorId = actorId ?? throw new ArgumentNullException(nameof(actorId));
            Choice = choice ?? throw new ArgumentNullException(nameof(choice));
        }

        public string ActorId { get; }

        public BattleActionChoice Choice { get; }
    }

    private sealed class PendingTargetSelection
    {
        public PendingTargetSelection(string actorId, BattleActionChoice choice)
        {
            if (choice is null)
            {
                throw new ArgumentNullException(nameof(choice));
            }

            ActorId = actorId ?? throw new ArgumentNullException(nameof(actorId));
            ActionKind = choice.ActionKind;
            ActionId = choice.ActionId;
            TargetMode = choice.TargetMode;
        }

        public string ActorId { get; }

        public BattleActionKind ActionKind { get; }

        public string? ActionId { get; }

        public BattleTargetMode TargetMode { get; }
    }
}