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

    public TeamPhaseBattleFlow(BattleTeam startingTeam)
    {
        if (startingTeam is not BattleTeam.Party and not BattleTeam.Enemy)
        {
            throw new ArgumentOutOfRangeException(nameof(startingTeam), "Starting team must be Party or Enemy.");
        }

        _startingTeam = startingTeam;
        _currentTeam = startingTeam;
        _nextIndexInCurrentTeam = 0;
    }

    public BattleFlowStep Advance(BattleState state)
    {
        if (state is null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        EnsureInitialized(state);

        if (TryFindReadyActor(state, _currentTeam, _nextIndexInCurrentTeam, out string? actorId, out int actorIndex))
        {
            _nextIndexInCurrentTeam = actorIndex + 1;

            return new BattleFlowStep(
                hasAdvanced: true,
                readyActorId: actorId);
        }

        BattleTeam otherTeam = GetOpposingTeam(_currentTeam);

        if (TryFindReadyActor(state, otherTeam, 0, out actorId, out actorIndex))
        {
            _currentTeam = otherTeam;
            _nextIndexInCurrentTeam = actorIndex + 1;

            return new BattleFlowStep(
                hasAdvanced: true,
                readyActorId: actorId);
        }

        return new BattleFlowStep(
            hasAdvanced: false,
            readyActorId: null);
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
            if (combatant is not null && !combatant.IsDefeated)
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