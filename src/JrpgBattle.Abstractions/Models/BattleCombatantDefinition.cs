// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleCombatantDefinition
{
    public BattleCombatantDefinition(
        string id,
        string name,
        BattleTeam team,
        int maxHp)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Combatant id is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Combatant name is required.", nameof(name));
        }

        if (maxHp <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxHp), "Max HP must be greater than zero.");
        }

        Id = id;
        Name = name;
        Team = team;
        MaxHp = maxHp;
    }

    public string Id { get; }

    public string Name { get; }

    public BattleTeam Team { get; }

    public int MaxHp { get; }
}