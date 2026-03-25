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
    private readonly IEnemyTargetChooser _enemyTargetChooser;
    private readonly IBattleActionResolver _actionResolver;
    private readonly IBattleRewardCalculator _rewardCalculator;
    private readonly IPlayerChoiceBlockingPolicy _playerChoiceBlockingPolicy;
    private readonly BattleRuntimeState _runtimeState;

    public BattleRuntime(
        BattleDefinition definition,
        IBattleFlow flow,
        IEnemyActionChooser enemyActionChooser,
        IEnemyTargetChooser enemyTargetChooser,
        IBattleActionResolver actionResolver,
        IBattleRewardCalculator rewardCalculator,
        IPlayerChoiceBlockingPolicy playerChoiceBlockingPolicy)
    {
        if (definition is null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        _flow = flow ?? throw new ArgumentNullException(nameof(flow));
        _enemyActionChooser = enemyActionChooser ?? throw new ArgumentNullException(nameof(enemyActionChooser));
        _enemyTargetChooser = enemyTargetChooser ?? throw new ArgumentNullException(nameof(enemyTargetChooser));
        _actionResolver = actionResolver ?? throw new ArgumentNullException(nameof(actionResolver));
        _rewardCalculator = rewardCalculator ?? throw new ArgumentNullException(nameof(rewardCalculator));
        _playerChoiceBlockingPolicy = playerChoiceBlockingPolicy ?? throw new ArgumentNullException(nameof(playerChoiceBlockingPolicy));

        _runtimeState = new BattleRuntimeState(CreateInitialBattleState(definition));
    }

    public BattleRuntimeView GetView()
    {
        return new BattleRuntimeView(_runtimeState.BattleState);
    }

    public BattleAdvanceResult Advance()
    {
        if (_runtimeState.Phase == BattleRuntimePhase.Ended)
        {
            return new BattleAdvanceResult(
                hasChanged: false,
                isPlayerInputNeeded: false,
                actionResult: null,
                battleResult: null);
        }

        if (_runtimeState.Phase == BattleRuntimePhase.WaitingForPlayerChoice)
        {
            return AdvanceWhilePlayerChoicePending();
        }

        BattleFlowResult flowResult = _flow.Advance(_runtimeState.BattleState);

        if (string.IsNullOrWhiteSpace(flowResult.ReadyActorId))
        {
            return new BattleAdvanceResult(
                hasChanged: flowResult.HasChanged,
                isPlayerInputNeeded: false,
                actionResult: null,
                battleResult: null);
        }

        BattleCombatantState actor = _runtimeState.BattleState.Combatants.FirstOrDefault(c => c.Id == flowResult.ReadyActorId)
            ?? throw new InvalidOperationException($"Actor '{flowResult.ReadyActorId}' not found.");

        if (!actor.IsAlive)
        {
            throw new InvalidOperationException("Ready actor is not alive.");
        }

        if (actor.Team == BattleTeam.Party)
        {
            _runtimeState.SetPendingActor(actor.Id);
            _runtimeState.SetPhase(BattleRuntimePhase.WaitingForPlayerChoice);

            return new BattleAdvanceResult(
                hasChanged: true,
                isPlayerInputNeeded: true,
                actionResult: null,
                battleResult: null);
        }

        _flow.ConsumeReadyActor(actor.Id);

        BattleActionKind actionKind = _enemyActionChooser.ChooseAction(_runtimeState.BattleState, actor.Id);
        string targetId = _enemyTargetChooser.ChooseTargetId(_runtimeState.BattleState, actor.Id);

        BattleActionChoice action = new(
            actorId: actor.Id,
            actionKind: actionKind,
            targetId: targetId);

        BattleActionResult actionResult = _actionResolver.Resolve(_runtimeState.BattleState, action);

        return ApplyResolvedAction(actionResult);
    }

    public BattleAdvanceResult SubmitPlayerAction(BattleActionChoice action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (_runtimeState.Phase == BattleRuntimePhase.Ended)
        {
            throw new InvalidOperationException("Battle has already ended.");
        }

        if (_runtimeState.Phase != BattleRuntimePhase.WaitingForPlayerChoice)
        {
            throw new InvalidOperationException("Battle is not waiting for a player choice.");
        }

        if (string.IsNullOrWhiteSpace(_runtimeState.PendingActorId))
        {
            throw new InvalidOperationException("No player actor is pending.");
        }

        if (!string.Equals(action.ActorId, _runtimeState.PendingActorId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Submitted action actor does not match the pending player actor.");
        }

        BattleCombatantState actor = _runtimeState.BattleState.Combatants.FirstOrDefault(c => c.Id == action.ActorId)
            ?? throw new InvalidOperationException($"Actor '{action.ActorId}' not found.");

        if (actor.Team != BattleTeam.Party)
        {
            throw new InvalidOperationException("Submitted player action must belong to the party.");
        }

        _flow.ConsumeReadyActor(actor.Id);

        BattleActionResult actionResult = _actionResolver.Resolve(_runtimeState.BattleState, action);

        return ApplyResolvedAction(actionResult);
    }

    private BattleAdvanceResult AdvanceWhilePlayerChoicePending()
    {
        BattleRuntimeView runtimeView = GetView();
        bool shouldBlock = _playerChoiceBlockingPolicy.ShouldBlockAdvance(runtimeView);

        if (shouldBlock)
        {
            return new BattleAdvanceResult(
                hasChanged: false,
                isPlayerInputNeeded: true,
                actionResult: null,
                battleResult: null);
        }

        BattleFlowResult flowResult = _flow.Advance(_runtimeState.BattleState);

        if (string.IsNullOrWhiteSpace(flowResult.ReadyActorId))
        {
            return new BattleAdvanceResult(
                hasChanged: flowResult.HasChanged,
                isPlayerInputNeeded: true,
                actionResult: null,
                battleResult: null);
        }

        BattleCombatantState actor = _runtimeState.BattleState.Combatants.FirstOrDefault(c => c.Id == flowResult.ReadyActorId)
            ?? throw new InvalidOperationException($"Actor '{flowResult.ReadyActorId}' not found.");

        if (!actor.IsAlive)
        {
            throw new InvalidOperationException("Ready actor is not alive.");
        }

        if (actor.Team == BattleTeam.Party)
        {
            return new BattleAdvanceResult(
                hasChanged: flowResult.HasChanged,
                isPlayerInputNeeded: true,
                actionResult: null,
                battleResult: null);
        }

        _flow.ConsumeReadyActor(actor.Id);

        BattleActionKind actionKind = _enemyActionChooser.ChooseAction(_runtimeState.BattleState, actor.Id);
        string targetId = _enemyTargetChooser.ChooseTargetId(_runtimeState.BattleState, actor.Id);

        BattleActionChoice action = new(
            actorId: actor.Id,
            actionKind: actionKind,
            targetId: targetId);

        BattleActionResult actionResult = _actionResolver.Resolve(_runtimeState.BattleState, action);

        return ApplyResolvedAction(actionResult, playerInputStillNeeded: true);
    }

    private BattleAdvanceResult ApplyResolvedAction(
        BattleActionResult actionResult,
        bool playerInputStillNeeded = false)
    {
        if (actionResult.ActionKind == BattleActionKind.Escape)
        {
            BattleState escapedBattleState = new(
                combatants: _runtimeState.BattleState.Combatants,
                isEnded: true,
                outcome: BattleOutcome.Escaped);

            _runtimeState.SetBattleState(escapedBattleState);
            _runtimeState.SetPendingActor(null);
            _runtimeState.SetPhase(BattleRuntimePhase.Ended);

            return new BattleAdvanceResult(
                hasChanged: true,
                isPlayerInputNeeded: false,
                actionResult: actionResult,
                battleResult: new BattleResult(BattleOutcome.Escaped, reward: null));
        }

        BattleState updatedBattleState = ApplyActionResult(_runtimeState.BattleState, actionResult);
        _runtimeState.SetBattleState(updatedBattleState);

        BattleResult? battleResult = TryBuildFinalResult(updatedBattleState);

        if (battleResult is not null)
        {
            _runtimeState.SetPendingActor(null);
            _runtimeState.SetPhase(BattleRuntimePhase.Ended);

            return new BattleAdvanceResult(
                hasChanged: true,
                isPlayerInputNeeded: false,
                actionResult: actionResult,
                battleResult: battleResult);
        }

        if (playerInputStillNeeded && !string.IsNullOrWhiteSpace(_runtimeState.PendingActorId))
        {
            _runtimeState.SetPhase(BattleRuntimePhase.WaitingForPlayerChoice);

            return new BattleAdvanceResult(
                hasChanged: true,
                isPlayerInputNeeded: true,
                actionResult: actionResult,
                battleResult: null);
        }

        _runtimeState.SetPendingActor(null);
        _runtimeState.SetPhase(BattleRuntimePhase.Advancing);

        return new BattleAdvanceResult(
            hasChanged: true,
            isPlayerInputNeeded: false,
            actionResult: actionResult,
            battleResult: null);
    }

    private BattleResult? TryBuildFinalResult(BattleState state)
    {
        bool partyAlive = state.Combatants.Any(c => c.Team == BattleTeam.Party && c.IsAlive);
        bool enemiesAlive = state.Combatants.Any(c => c.Team == BattleTeam.Enemy && c.IsAlive);

        if (partyAlive && enemiesAlive)
        {
            return null;
        }

        BattleOutcome outcome = partyAlive
            ? BattleOutcome.Victory
            : BattleOutcome.Defeat;

        BattleState endedState = new(
            combatants: state.Combatants,
            isEnded: true,
            outcome: outcome);

        _runtimeState.SetBattleState(endedState);

        BattleReward? reward = null;
        if (outcome == BattleOutcome.Victory)
        {
            reward = _rewardCalculator.Calculate(endedState);
        }

        return new BattleResult(outcome, reward);
    }

    private static BattleState ApplyActionResult(BattleState state, BattleActionResult actionResult)
    {
        List<BattleCombatantState> updatedCombatants = state.Combatants
            .Select(c =>
            {
                if (!string.Equals(c.Id, actionResult.TargetId, StringComparison.Ordinal))
                {
                    return c;
                }

                int newHp = Math.Max(0, c.CurrentHp - actionResult.DamageDealt);

                return new BattleCombatantState(
                    id: c.Id,
                    name: c.Name,
                    team: c.Team,
                    currentHp: newHp,
                    maxHp: c.MaxHp);
            })
            .ToList();

        return new BattleState(
            combatants: updatedCombatants,
            isEnded: false,
            outcome: BattleOutcome.None);
    }

    private static BattleState CreateInitialBattleState(BattleDefinition definition)
    {
        List<BattleCombatantState> combatants = new();

        foreach (BattleCombatantDefinition combatant in definition.PartyCombatants)
        {
            combatants.Add(new BattleCombatantState(
                id: combatant.Id,
                name: combatant.Name,
                team: combatant.Team,
                currentHp: combatant.MaxHp,
                maxHp: combatant.MaxHp));
        }

        foreach (BattleCombatantDefinition combatant in definition.EnemyCombatants)
        {
            combatants.Add(new BattleCombatantState(
                id: combatant.Id,
                name: combatant.Name,
                team: combatant.Team,
                currentHp: combatant.MaxHp,
                maxHp: combatant.MaxHp));
        }

        return new BattleState(
            combatants: combatants,
            isEnded: false,
            outcome: BattleOutcome.None);
    }
}