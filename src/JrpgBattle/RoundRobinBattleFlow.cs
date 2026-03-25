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
    private string? _readyActorId;
    private int _readyActorIndex;

    public RoundRobinBattleFlow(BattleTeam startingTeam)
    {
        if (startingTeam is not BattleTeam.Party and not BattleTeam.Enemy)
        {
            throw new ArgumentOutOfRangeException(nameof(startingTeam), "Starting team must be Party or Enemy.");
        }

        _startingTeam = startingTeam;
        _nextIndex = 0;
        _readyActorIndex = -1;
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

        if (_orderedActorIds!.Count == 0)
        {
            return new BattleFlowResult(
                HasChanged: false,
                ReadyActorId: null);
        }

        for (int i = 0; i < _orderedActorIds.Count; i++)
        {
            int candidateIndex = (_nextIndex + i) % _orderedActorIds.Count;
            string candidateActorId = _orderedActorIds[candidateIndex];

            BattleCombatantState? combatant = state.Combatants.FirstOrDefault(c => c.Id == candidateActorId);
            if (combatant is not null && combatant.IsAlive)
            {
                _readyActorId = candidateActorId;
                _readyActorIndex = candidateIndex;

                return new BattleFlowResult(
                    HasChanged: true,
                    ReadyActorId: _readyActorId);
            }
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

        _nextIndex = _orderedActorIds!.Count == 0
            ? 0
            : (_readyActorIndex + 1) % _orderedActorIds.Count;

        _readyActorId = null;
        _readyActorIndex = -1;
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