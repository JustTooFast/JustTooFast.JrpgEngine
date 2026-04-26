// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleStateView
{
    public BattleStateView(IEnumerable<BattleActorView> actors)
    {
        if (actors is null)
        {
            throw new ArgumentNullException(nameof(actors));
        }

        BattleActorView[] materializedActors = actors.ToArray();

        if (materializedActors.Length == 0)
        {
            throw new ArgumentException("At least one actor view is required.", nameof(actors));
        }

        if (materializedActors.Any(static actor => actor is null))
        {
            throw new ArgumentException("Actors cannot contain null entries.", nameof(actors));
        }

        string[] duplicateIds = materializedActors
            .GroupBy(static actor => actor.ActorId, StringComparer.Ordinal)
            .Where(static group => group.Count() > 1)
            .Select(static group => group.Key)
            .ToArray();

        if (duplicateIds.Length > 0)
        {
            throw new ArgumentException("Actor ids must be unique.", nameof(actors));
        }

        Actors = new ReadOnlyCollection<BattleActorView>(materializedActors);
    }

    public IReadOnlyList<BattleActorView> Actors { get; }
}