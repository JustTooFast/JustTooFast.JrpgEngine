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
    private readonly BattleDefinition _definition;
    private readonly IReadOnlyDictionary<string, BattleActorDefinition> _actorDefinitions;

    private PendingExecution? _pendingExecution;

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

        _definition = definition;
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

        Dictionary<string, BattleExtendedData> actorExtendedData = actorDefinitions
            .ToDictionary(
                static actor => actor.Id,
                static actor => actor.ExtendedData,
                StringComparer.Ordinal);

        _runtimeState = new BattleRuntimeState(
            new BattleState(actors),
            actorExtendedData);
    }

    public BattleRuntimeView GetView()
    {
        BattleActorView[] actorViews = _runtimeState.BattleState.Actors
            .Select(actor => new BattleActorView(
                actorId: actor.Id,
                displayName: actor.Name,
                team: actor.Team,
                currentHp: actor.CurrentHp,
                maxHp: actor.MaxHp,
                isDefeated: actor.IsDefeated,
                extendedData: _runtimeState.GetActorExtendedData(actor.Id)))
            .ToArray();

        return new BattleRuntimeView(
            new BattleStateView(actorViews),
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

            BattleInputAction selectedAction = FindRequestedAction(actorId, submittedChoice.ActionId);

            _runtimeState.SetPendingPlayerChoice(null);
            _runtimeState.SetInputRequest(null);

            StagePendingExecution(actorId, selectedAction, submittedChoice.TargetIds);
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
            _runtimeState.SetInputRequest(CreateInputRequest(actor.Id));
            return;
        }

        BattleActionChoice automatedChoice =
            _enemyActionChooser.ChooseAction(
                new BattleChooserContext(
                    definition: _definition,
                    state: _runtimeState.BattleState,
                    actorId: actor.Id));

        ValidateChosenEnemyAction(actor.Id, automatedChoice);

        BattleInputAction automatedAction = FindAvailableAction(actor.Id, automatedChoice.ActionId);

        StagePendingExecution(actor.Id, automatedAction, automatedChoice.TargetIds);
    }

    private BattleInputRequest CreateInputRequest(string actorId)
    {
        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        }

        BattleInputAction[] actions = BuildAvailableActions(actorId).ToArray();

        return new BattleInputRequest(
            actorId: actorId,
            actions: actions);
    }

    private IEnumerable<BattleInputAction> BuildAvailableActions(string actorId)
    {
        BattleActorDefinition actorDefinition = FindActorDefinition(actorId);
        BattleTeamDefinition teamDefinition = FindTeamDefinition(actorDefinition.Team);

        foreach (BattleActionDefinition allowedAction in actorDefinition.AllowedActions)
        {
            if (ShouldExposeBuiltInAction(allowedAction))
            {
                yield return CreateBuiltInInputAction(allowedAction.ActionKind);
            }
        }

        foreach (BattleAbilityDefinition spell in actorDefinition.Spells)
        {
            yield return new BattleInputAction(
                actionId: spell.Id,
                displayText: spell.Name,
                actionKind: BattleActionKind.Magic,
                category: null,
                targetMode: spell.TargetMode,
                isEnabled: true);
        }

        foreach (BattleAbilityDefinition skill in actorDefinition.Skills)
        {
            yield return new BattleInputAction(
                actionId: skill.Id,
                displayText: skill.Name,
                actionKind: BattleActionKind.Skill,
                category: null,
                targetMode: skill.TargetMode,
                isEnabled: true);
        }

        foreach (BattleAbilityDefinition item in teamDefinition.Items)
        {
            yield return new BattleInputAction(
                actionId: item.Id,
                displayText: item.Name,
                actionKind: BattleActionKind.Item,
                category: null,
                targetMode: item.TargetMode,
                isEnabled: true);
        }
    }

    private bool ShouldExposeBuiltInAction(BattleActionDefinition actionDefinition)
    {
        if (actionDefinition is null)
        {
            throw new ArgumentNullException(nameof(actionDefinition));
        }

        if (actionDefinition.ActionKind == BattleActionKind.Escape && !_definition.Configuration.CanEscape)
        {
            return false;
        }

        return actionDefinition.ActionKind switch
        {
            BattleActionKind.Attack => true,
            BattleActionKind.Defend => true,
            BattleActionKind.Escape => true,
            BattleActionKind.Wait => true,
            BattleActionKind.Magic => false,
            BattleActionKind.Skill => false,
            BattleActionKind.Item => false,
            _ => false
        };
    }

    private static BattleInputAction CreateBuiltInInputAction(BattleActionKind actionKind)
    {
        return actionKind switch
        {
            BattleActionKind.Attack => new BattleInputAction(
                actionId: "attack",
                displayText: "Attack",
                actionKind: BattleActionKind.Attack,
                category: null,
                targetMode: BattleTargetMode.SingleTarget,
                isEnabled: true),

            BattleActionKind.Defend => new BattleInputAction(
                actionId: "defend",
                displayText: "Defend",
                actionKind: BattleActionKind.Defend,
                category: null,
                targetMode: BattleTargetMode.None,
                isEnabled: true),

            BattleActionKind.Escape => new BattleInputAction(
                actionId: "escape",
                displayText: "Escape",
                actionKind: BattleActionKind.Escape,
                category: null,
                targetMode: BattleTargetMode.None,
                isEnabled: true),

            BattleActionKind.Wait => new BattleInputAction(
                actionId: "wait",
                displayText: "Wait",
                actionKind: BattleActionKind.Wait,
                category: null,
                targetMode: BattleTargetMode.None,
                isEnabled: true),

            _ => throw new NotSupportedException(
                $"Built-in input action creation is not supported for action kind '{actionKind}'.")
        };
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

        return new BattleFlowState(
            configuration: _definition.Configuration,
            battleState: _runtimeState.BattleState,
            actorStates: actorStates);
    }

    private void StagePendingExecution(
        string actorId,
        BattleInputAction selectedAction,
        IReadOnlyList<string>? targetIds)
    {
        _pendingExecution = new PendingExecution(actorId, selectedAction, targetIds);
    }

    private void ApplyPendingExecution()
    {
        PendingExecution pending = _pendingExecution
            ?? throw new InvalidOperationException("No pending execution exists.");

        _pendingExecution = null;

        ExecuteAction(pending.ActorId, pending.Action, pending.TargetIds);
    }

    private void ExecuteAction(
        string actorId,
        BattleInputAction selectedAction,
        IReadOnlyList<string>? targetIds)
    {
        var occurrences = new List<BattleOccurrence>();

        occurrences.Add(new ActionStartedOccurrence(
            actorId,
            selectedAction.ActionKind,
            selectedAction.ActionId));

        BattleResolution resolution =
            _actionResolver.Resolve(
                new BattleResolverContext(
                    definition: _definition,
                    state: _runtimeState.BattleState,
                    actorId: actorId,
                    action: new BattleActionChoice(
                        actionId: selectedAction.ActionId,
                        targetMode: selectedAction.TargetMode,
                        targetIds: targetIds)));

        ApplyResolution(actorId, resolution, occurrences);

        occurrences.Add(new ActionResolvedOccurrence(
            actorId,
            selectedAction.ActionKind,
            selectedAction.ActionId));

        FinalizeBattleResultIfNeeded();

        _runtimeState.SetOccurrences(occurrences);
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

        BattleInputAction requestedAction = FindRequestedAction(actorId, choice.ActionId);

        if (!requestedAction.IsEnabled)
        {
            throw new InvalidOperationException($"Submitted action '{choice.ActionId}' is currently disabled.");
        }

        if (requestedAction.TargetMode != choice.TargetMode)
        {
            throw new InvalidOperationException("Submitted action target mode does not match the input request.");
        }

        ValidateTargetIds(choice.TargetMode, choice.TargetIds, actorId);
    }

    private BattleInputAction FindRequestedAction(string actorId, string actionId)
    {
        if (_runtimeState.CurrentInputRequest is null)
        {
            throw new InvalidOperationException("No input request is active.");
        }

        if (!string.Equals(_runtimeState.CurrentInputRequest.ActorId, actorId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Requested actor does not match the active input request.");
        }

        BattleInputAction? action = _runtimeState.CurrentInputRequest.Actions
            .FirstOrDefault(a => string.Equals(a.ActionId, actionId, StringComparison.Ordinal));

        if (action is null)
        {
            throw new InvalidOperationException(
                $"Submitted action '{actionId}' does not exist in the current input request.");
        }

        return action;
    }

    private static void ValidateTargetIds(
        BattleTargetMode targetMode,
        IReadOnlyList<string>? targetIds,
        string actorId)
    {
        switch (targetMode)
        {
            case BattleTargetMode.None:
                if (targetIds is not null && targetIds.Count > 0)
                {
                    throw new InvalidOperationException(
                        $"Actor '{actorId}' submitted unexpected target ids for a non-targeted action.");
                }

                break;

            case BattleTargetMode.SingleTarget:
                if (targetIds is null || targetIds.Count != 1)
                {
                    throw new InvalidOperationException(
                        $"Actor '{actorId}' must provide exactly one target id for a single-target action.");
                }

                break;

            case BattleTargetMode.AllAllies:
            case BattleTargetMode.AllEnemies:
                if (targetIds is not null && targetIds.Count > 0)
                {
                    throw new InvalidOperationException(
                        $"Actor '{actorId}' submitted explicit target ids for target mode '{targetMode}'.");
                }

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(targetMode), targetMode, "Unsupported target mode.");
        }
    }

    private void ValidateChosenEnemyAction(string actorId, BattleActionChoice choice)
    {
        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        }

        if (choice is null)
        {
            throw new InvalidOperationException($"Enemy chooser returned no action for actor '{actorId}'.");
        }

        BattleActorState actor = FindActor(actorId);

        if (actor.IsDefeated)
        {
            throw new InvalidOperationException($"Actor '{actorId}' cannot choose an action because they are defeated.");
        }

        BattleInputAction selectedAction = FindAvailableAction(actorId, choice.ActionId);

        if (!selectedAction.IsEnabled)
        {
            throw new InvalidOperationException(
                $"Actor '{actorId}' chose action '{choice.ActionId}', but that action is currently disabled.");
        }

        if (selectedAction.TargetMode != choice.TargetMode)
        {
            throw new InvalidOperationException(
                $"Actor '{actorId}' chose action '{choice.ActionId}' with invalid target mode '{choice.TargetMode}'.");
        }

        ValidateTargetIds(choice.TargetMode, choice.TargetIds, actorId);
    }

    private BattleInputAction FindAvailableAction(string actorId, string actionId)
    {
        BattleInputAction? action = BuildAvailableActions(actorId)
            .FirstOrDefault(a => string.Equals(a.ActionId, actionId, StringComparison.Ordinal));

        if (action is null)
        {
            throw new InvalidOperationException(
                $"Actor '{actorId}' chose action '{actionId}', but that action is not available.");
        }

        return action;
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

    private BattleTeamDefinition FindTeamDefinition(BattleTeam team)
    {
        BattleTeamDefinition? teamDefinition = _definition.TeamDefinitions
            .FirstOrDefault(d => d.Team == team);

        if (teamDefinition is null)
        {
            throw new InvalidOperationException($"Team definition for '{team}' was not found.");
        }

        return teamDefinition;
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
        public PendingExecution(
            string actorId,
            BattleInputAction action,
            IReadOnlyList<string>? targetIds)
        {
            if (string.IsNullOrWhiteSpace(actorId))
            {
                throw new ArgumentException("Actor id is required.", nameof(actorId));
            }

            ActorId = actorId;
            Action = action ?? throw new ArgumentNullException(nameof(action));
            TargetIds = targetIds;
        }

        public string ActorId { get; }

        public BattleInputAction Action { get; }

        public IReadOnlyList<string>? TargetIds { get; }
    }
}