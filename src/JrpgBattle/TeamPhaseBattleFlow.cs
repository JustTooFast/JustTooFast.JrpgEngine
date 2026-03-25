// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

public sealed class TeamPhaseBattleFlow : IBattleFlow
{
    private readonly BattleTeam _startingTeam;

    private List<string>? _partyActorIds;
    private List<string>? _enemyActorIds;

    private BattleTeam _currentTeam;
    private int _nextIndexInCurrentTeam;

    private string? _readyActorId;
    private int _readyActorIndexInCurrentTeam;

    public TeamPhaseBattleFlow(BattleTeam startingTeam)
    {
        if (startingTeam is not BattleTeam.Party and not BattleTeam.Enemy)
        {
            throw new ArgumentOutOfRangeException(nameof(startingTeam), "Starting team must be Party or Enemy.");
        }

        _startingTeam = startingTeam;
        _currentTeam = startingTeam;
        _nextIndexInCurrentTeam = 0;
        _readyActorIndexInCurrentTeam = -1;
    }

    public BattleFlowResult Advance(BattleState state)
    {
        if (state is null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        EnsureInitialized(state);

        if (!string.IsNullOrWhiteSpace(_readyActorId))
        {
            return new BattleFlowResult(
                HasChanged: false,
                ReadyActorId: _readyActorId);
        }

        if (TryFindReadyActor(state, _currentTeam, _nextIndexInCurrentTeam, out string? actorId, out int actorIndex))
        {
            _readyActorId = actorId;
            _readyActorIndexInCurrentTeam = actorIndex;

            return new BattleFlowResult(
                HasChanged: true,
                ReadyActorId: _readyActorId);
        }

        BattleTeam otherTeam = GetOpposingTeam(_currentTeam);

        if (TryFindReadyActor(state, otherTeam, 0, out actorId, out actorIndex))
        {
            _currentTeam = otherTeam;
            _nextIndexInCurrentTeam = 0;
            _readyActorId = actorId;
            _readyActorIndexInCurrentTeam = actorIndex;

            return new BattleFlowResult(
                HasChanged: true,
                ReadyActorId: _readyActorId);
        }

        return new BattleFlowResult(
            HasChanged: false,
            ReadyActorId: null);
    }

    public void ConsumeReadyActor(string actorId)
    {
        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        }

        if (!string.Equals(_readyActorId, actorId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Ready actor does not match the actor being consumed.");
        }

        _nextIndexInCurrentTeam = _readyActorIndexInCurrentTeam + 1;
        _readyActorId = null;
        _readyActorIndexInCurrentTeam = -1;
    }

    private void EnsureInitialized(BattleState state)
    {
        if (_partyActorIds is not null && _enemyActorIds is not null)
        {
            return;
        }

        _partyActorIds = state.Combatants
            .Where(c => c.Team == BattleTeam.Party)
            .Select(c => c.Id)
            .ToList();

        _enemyActorIds = state.Combatants
            .Where(c => c.Team == BattleTeam.Enemy)
            .Select(c => c.Id)
            .ToList();
    }

    private bool TryFindReadyActor(
        BattleState state,
        BattleTeam team,
        int startIndex,
        out string? actorId,
        out int actorIndex)
    {
        IReadOnlyList<string> actorIds = team == BattleTeam.Party
            ? _partyActorIds!
            : _enemyActorIds!;

        for (int i = startIndex; i < actorIds.Count; i++)
        {
            string candidateActorId = actorIds[i];

            BattleCombatantState? combatant = state.Combatants.FirstOrDefault(c => c.Id == candidateActorId);
            if (combatant is not null && combatant.IsAlive)
            {
                actorId = candidateActorId;
                actorIndex = i;
                return true;
            }
        }

        actorId = null;
        actorIndex = -1;
        return false;
    }

    private static BattleTeam GetOpposingTeam(BattleTeam team)
    {
        return team == BattleTeam.Party
            ? BattleTeam.Enemy
            : BattleTeam.Party;
    }
}