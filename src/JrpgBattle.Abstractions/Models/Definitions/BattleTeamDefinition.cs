// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleTeamDefinition
{
    public BattleTeamDefinition(
        BattleTeam team,
        IReadOnlyList<BattleAbilityDefinition> items,
        BattleExtendedData extendedData)
    {
        if (team is not BattleTeam.Party and not BattleTeam.Enemy)
        {
            throw new ArgumentOutOfRangeException(nameof(team), "Team must be Party or Enemy.");
        }

        if (items is null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        if (extendedData is null)
        {
            throw new ArgumentNullException(nameof(extendedData));
        }

        if (items.Any(static item => item is null))
        {
            throw new ArgumentException("Items cannot contain null entries.", nameof(items));
        }

        string[] duplicateItemIds = items
            .GroupBy(static item => item.Id, StringComparer.Ordinal)
            .Where(static group => group.Count() > 1)
            .Select(static group => group.Key)
            .ToArray();

        if (duplicateItemIds.Length > 0)
        {
            throw new ArgumentException(
                $"Item ids must be unique per team: {string.Join(", ", duplicateItemIds)}.",
                nameof(items));
        }

        Team = team;
        Items = new ReadOnlyCollection<BattleAbilityDefinition>(items.ToArray());
        ExtendedData = extendedData;
    }

    public BattleTeam Team { get; }

    public IReadOnlyList<BattleAbilityDefinition> Items { get; }

    public BattleExtendedData ExtendedData { get; }
}