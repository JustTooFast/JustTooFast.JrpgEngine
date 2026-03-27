// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleCombatantState
{
    public BattleCombatantState(
        string id,
        string name,
        BattleTeam team,
        int currentHp,
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

        if (currentHp < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(currentHp), "Current HP cannot be negative.");
        }

        if (currentHp > maxHp)
        {
            throw new ArgumentOutOfRangeException(nameof(currentHp), "Current HP cannot exceed Max HP.");
        }

        Id = id;
        Name = name;
        Team = team;
        CurrentHp = currentHp;
        MaxHp = maxHp;
    }

    public string Id { get; }

    public string Name { get; }

    public BattleTeam Team { get; }

    public int CurrentHp { get; }

    public int MaxHp { get; }

    public bool IsDefeated => CurrentHp == 0;
}