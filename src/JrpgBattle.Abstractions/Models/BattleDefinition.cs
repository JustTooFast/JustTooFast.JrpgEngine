// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleDefinition
{
    public BattleDefinition(
        IReadOnlyList<BattleCombatantDefinition> partyCombatants,
        IReadOnlyList<BattleCombatantDefinition> enemyCombatants)
    {
        PartyCombatants = partyCombatants ?? throw new ArgumentNullException(nameof(partyCombatants));
        EnemyCombatants = enemyCombatants ?? throw new ArgumentNullException(nameof(enemyCombatants));

        if (PartyCombatants.Count == 0)
        {
            throw new ArgumentException("At least one party combatant is required.", nameof(partyCombatants));
        }

        if (EnemyCombatants.Count == 0)
        {
            throw new ArgumentException("At least one enemy combatant is required.", nameof(enemyCombatants));
        }
    }

    public IReadOnlyList<BattleCombatantDefinition> PartyCombatants { get; }

    public IReadOnlyList<BattleCombatantDefinition> EnemyCombatants { get; }
}