// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleState
{
    public BattleState(IReadOnlyList<BattleCombatantState> combatants)
    {
        Combatants = combatants ?? throw new ArgumentNullException(nameof(combatants));

        if (Combatants.Count == 0)
        {
            throw new ArgumentException("At least one combatant is required.", nameof(combatants));
        }
    }

    public IReadOnlyList<BattleCombatantState> Combatants { get; }
}