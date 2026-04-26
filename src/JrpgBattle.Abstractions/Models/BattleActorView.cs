// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleActorView
{
    public BattleActorView(
        string actorId,
        string displayName,
        BattleTeam team,
        int currentHp,
        int maxHp,
        bool isDefeated,
        BattleExtendedData extendedData)
    {
        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name is required.", nameof(displayName));
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

        if (extendedData is null)
        {
            throw new ArgumentNullException(nameof(extendedData));
        }

        ActorId = actorId;
        DisplayName = displayName;
        Team = team;
        CurrentHp = currentHp;
        MaxHp = maxHp;
        IsDefeated = isDefeated;
        ExtendedData = extendedData;
    }

    public string ActorId { get; }

    public string DisplayName { get; }

    public BattleTeam Team { get; }

    public int CurrentHp { get; }

    public int MaxHp { get; }

    public bool IsDefeated { get; }

    public BattleExtendedData ExtendedData { get; }
}