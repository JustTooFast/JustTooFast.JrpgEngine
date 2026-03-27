// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

public sealed class RoundRobinBattleFlow : IBattleFlow
{
    private readonly BattleTeam _startingTeam;

    private List<string>? _orderedActorIds;
    private int _nextIndex;

    public RoundRobinBattleFlow(BattleTeam startingTeam)
    {
        if (startingTeam is not BattleTeam.Party and not BattleTeam.Enemy)
        {
            throw new ArgumentOutOfRangeException(nameof(startingTeam), "Starting team must be Party or Enemy.");
        }

        _startingTeam = startingTeam;
        _nextIndex = 0;
    }

    public BattleFlowStep Advance(BattleState state)
    {
        if (state is null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        EnsureInitialized(state);

        if (_orderedActorIds!.Count == 0)
        {
            return new BattleFlowStep(
                hasAdvanced: false,
                readyActorId: null);
        }

        for (int i = 0; i < _orderedActorIds.Count; i++)
        {
            int candidateIndex = (_nextIndex + i) % _orderedActorIds.Count;
            string candidateActorId = _orderedActorIds[candidateIndex];

            BattleCombatantState? combatant = state.Combatants.FirstOrDefault(c => c.Id == candidateActorId);
            if (combatant is not null && !combatant.IsDefeated)
            {
                _nextIndex = (candidateIndex + 1) % _orderedActorIds.Count;

                return new BattleFlowStep(
                    hasAdvanced: true,
                    readyActorId: candidateActorId);
            }
        }

        return new BattleFlowStep(
            hasAdvanced: false,
            readyActorId: null);
    }

    private void EnsureInitialized(BattleState state)
    {
        if (_orderedActorIds is not null)
        {
            return;
        }

        List<string> partyIds = state.Combatants
            .Where(c => c.Team == BattleTeam.Party)
            .Select(c => c.Id)
            .ToList();

        List<string> enemyIds = state.Combatants
            .Where(c => c.Team == BattleTeam.Enemy)
            .Select(c => c.Id)
            .ToList();

        _orderedActorIds = BuildInterleavedOrder(
            partyIds,
            enemyIds,
            _startingTeam);
    }

    private static List<string> BuildInterleavedOrder(
        IReadOnlyList<string> partyIds,
        IReadOnlyList<string> enemyIds,
        BattleTeam startingTeam)
    {
        List<string> ordered = new();

        IReadOnlyList<string> first = startingTeam == BattleTeam.Party ? partyIds : enemyIds;
        IReadOnlyList<string> second = startingTeam == BattleTeam.Party ? enemyIds : partyIds;

        int maxCount = Math.Max(first.Count, second.Count);

        for (int i = 0; i < maxCount; i++)
        {
            if (i < first.Count)
            {
                ordered.Add(first[i]);
            }

            if (i < second.Count)
            {
                ordered.Add(second[i]);
            }
        }

        return ordered;
    }
}