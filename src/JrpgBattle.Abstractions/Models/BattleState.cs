// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleState
{
    public BattleState(
        IReadOnlyList<BattleCombatantState> combatants,
        bool isEnded,
        BattleOutcome outcome)
    {
        Combatants = combatants ?? throw new ArgumentNullException(nameof(combatants));

        if (Combatants.Count == 0)
        {
            throw new ArgumentException("At least one combatant is required.", nameof(combatants));
        }

        if (!isEnded && outcome != BattleOutcome.None)
        {
            throw new ArgumentException("Outcome must be None when the battle has not ended.", nameof(outcome));
        }

        if (isEnded && outcome == BattleOutcome.None)
        {
            throw new ArgumentException("Outcome must be set when the battle has ended.", nameof(outcome));
        }

        Combatants = combatants;
        IsEnded = isEnded;
        Outcome = outcome;
    }

    public IReadOnlyList<BattleCombatantState> Combatants { get; }

    public bool IsEnded { get; }

    public BattleOutcome Outcome { get; }
}