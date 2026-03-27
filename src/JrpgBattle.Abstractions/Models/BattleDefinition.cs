// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleDefinition
{
    public BattleDefinition(
        IReadOnlyList<BattleActorDefinition> partyActors,
        IReadOnlyList<BattleActorDefinition> enemyActors)
    {
        if (partyActors is null)
        {
            throw new ArgumentNullException(nameof(partyActors));
        }

        if (enemyActors is null)
        {
            throw new ArgumentNullException(nameof(enemyActors));
        }

        if (partyActors.Count == 0)
        {
            throw new ArgumentException("At least one party actor is required.", nameof(partyActors));
        }

        if (enemyActors.Count == 0)
        {
            throw new ArgumentException("At least one enemy actor is required.", nameof(enemyActors));
        }

        if (partyActors.Any(static c => c is null))
        {
            throw new ArgumentException("Party actors cannot contain null entries.", nameof(partyActors));
        }

        if (enemyActors.Any(static c => c is null))
        {
            throw new ArgumentException("Enemy actors cannot contain null entries.", nameof(enemyActors));
        }

        if (partyActors.Any(static c => c.Team != BattleTeam.Party))
        {
            throw new ArgumentException("All party actors must have Party team.", nameof(partyActors));
        }

        if (enemyActors.Any(static c => c.Team != BattleTeam.Enemy))
        {
            throw new ArgumentException("All enemy actors must have Enemy team.", nameof(enemyActors));
        }

        string[] duplicateIds = partyActors
            .Concat(enemyActors)
            .GroupBy(static c => c.Id, StringComparer.Ordinal)
            .Where(static g => g.Count() > 1)
            .Select(static g => g.Key)
            .ToArray();

        if (duplicateIds.Length > 0)
        {
            throw new ArgumentException("Actor ids must be unique across the whole battle.", nameof(partyActors));
        }

        PartyActors = new ReadOnlyCollection<BattleActorDefinition>(partyActors.ToArray());
        EnemyActors = new ReadOnlyCollection<BattleActorDefinition>(enemyActors.ToArray());
    }

    public IReadOnlyList<BattleActorDefinition> PartyActors { get; }

    public IReadOnlyList<BattleActorDefinition> EnemyActors { get; }
}