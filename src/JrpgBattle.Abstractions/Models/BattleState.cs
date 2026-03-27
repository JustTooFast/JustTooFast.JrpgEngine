// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleState
{
    public BattleState(IReadOnlyList<BattleActorState> actors)
    {
        if (actors is null)
        {
            throw new ArgumentNullException(nameof(actors));
        }

        if (actors.Count == 0)
        {
            throw new ArgumentException("At least one actor is required.", nameof(actors));
        }

        if (actors.Any(static c => c is null))
        {
            throw new ArgumentException("Actors cannot contain null entries.", nameof(actors));
        }

        string[] duplicateIds = actors
            .GroupBy(static c => c.Id, StringComparer.Ordinal)
            .Where(static g => g.Count() > 1)
            .Select(static g => g.Key)
            .ToArray();

        if (duplicateIds.Length > 0)
        {
            throw new ArgumentException("Actor ids must be unique.", nameof(actors));
        }

        Actors = new ReadOnlyCollection<BattleActorState>(actors.ToArray());
    }

    public IReadOnlyList<BattleActorState> Actors { get; }
}