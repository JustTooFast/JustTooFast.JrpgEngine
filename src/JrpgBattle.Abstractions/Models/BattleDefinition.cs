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
        IReadOnlyList<BattleCombatantDefinition> partyCombatants,
        IReadOnlyList<BattleCombatantDefinition> enemyCombatants)
    {
        if (partyCombatants is null)
        {
            throw new ArgumentNullException(nameof(partyCombatants));
        }

        if (enemyCombatants is null)
        {
            throw new ArgumentNullException(nameof(enemyCombatants));
        }

        if (partyCombatants.Count == 0)
        {
            throw new ArgumentException("At least one party combatant is required.", nameof(partyCombatants));
        }

        if (enemyCombatants.Count == 0)
        {
            throw new ArgumentException("At least one enemy combatant is required.", nameof(enemyCombatants));
        }

        if (partyCombatants.Any(static c => c is null))
        {
            throw new ArgumentException("Party combatants cannot contain null entries.", nameof(partyCombatants));
        }

        if (enemyCombatants.Any(static c => c is null))
        {
            throw new ArgumentException("Enemy combatants cannot contain null entries.", nameof(enemyCombatants));
        }

        if (partyCombatants.Any(static c => c.Team != BattleTeam.Party))
        {
            throw new ArgumentException("All party combatants must have Party team.", nameof(partyCombatants));
        }

        if (enemyCombatants.Any(static c => c.Team != BattleTeam.Enemy))
        {
            throw new ArgumentException("All enemy combatants must have Enemy team.", nameof(enemyCombatants));
        }

        string[] duplicateIds = partyCombatants
            .Concat(enemyCombatants)
            .GroupBy(static c => c.Id, StringComparer.Ordinal)
            .Where(static g => g.Count() > 1)
            .Select(static g => g.Key)
            .ToArray();

        if (duplicateIds.Length > 0)
        {
            throw new ArgumentException("Combatant ids must be unique across the whole battle.", nameof(partyCombatants));
        }

        PartyCombatants = new ReadOnlyCollection<BattleCombatantDefinition>(partyCombatants.ToArray());
        EnemyCombatants = new ReadOnlyCollection<BattleCombatantDefinition>(enemyCombatants.ToArray());
    }

    public IReadOnlyList<BattleCombatantDefinition> PartyCombatants { get; }

    public IReadOnlyList<BattleCombatantDefinition> EnemyCombatants { get; }
}