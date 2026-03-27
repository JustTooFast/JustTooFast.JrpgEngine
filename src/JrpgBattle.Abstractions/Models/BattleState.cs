// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleState
{
    public BattleState(IReadOnlyList<BattleCombatantState> combatants)
    {
        if (combatants is null)
        {
            throw new ArgumentNullException(nameof(combatants));
        }

        if (combatants.Count == 0)
        {
            throw new ArgumentException("At least one combatant is required.", nameof(combatants));
        }

        if (combatants.Any(static c => c is null))
        {
            throw new ArgumentException("Combatants cannot contain null entries.", nameof(combatants));
        }

        string[] duplicateIds = combatants
            .GroupBy(static c => c.Id, StringComparer.Ordinal)
            .Where(static g => g.Count() > 1)
            .Select(static g => g.Key)
            .ToArray();

        if (duplicateIds.Length > 0)
        {
            throw new ArgumentException("Combatant ids must be unique.", nameof(combatants));
        }

        Combatants = new ReadOnlyCollection<BattleCombatantState>(combatants.ToArray());
    }

    public IReadOnlyList<BattleCombatantState> Combatants { get; }
}