// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.Definitions;
using JustTooFast.JrpgEngine.State;

namespace JustTooFast.JrpgEngine.Systems;

public sealed class EncounterService
{
    private readonly Random _random = new();

    public EncounterDef? TryTriggerEncounter(
        GameState gameState,
        MapDef mapDef,
        DefinitionDatabase definitions)
    {
        if (!mapDef.EncountersEnabled)
        {
            return null;
        }

        gameState.EncounterSteps++;

        var roll = _random.Next(0, 100);

        if (roll >= mapDef.EncounterRate)
        {
            return null;
        }

        if (!definitions.EncounterTables.TryGetValue(mapDef.EncounterTableId!, out var table))
        {
            throw new InvalidOperationException("Encounter table missing.");
        }

        return SelectEncounter(table, definitions);
    }

    private EncounterDef SelectEncounter(
        EncounterTableDef table,
        DefinitionDatabase definitions)
    {
        var totalWeight = 0;

        foreach (var entry in table.Entries)
        {
            totalWeight += entry.Weight;
        }

        var roll = _random.Next(totalWeight);

        var cumulative = 0;

        foreach (var entry in table.Entries)
        {
            cumulative += entry.Weight;

            if (roll < cumulative)
            {
                return definitions.Encounters[entry.EncounterId];
            }
        }

        throw new InvalidOperationException("Encounter selection failed.");
    }
}