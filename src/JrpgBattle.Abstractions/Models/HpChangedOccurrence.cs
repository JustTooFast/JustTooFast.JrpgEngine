// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record HpChangedOccurrence : BattleOccurrence
{
    public HpChangedOccurrence(
        string actorId,
        int previousHp,
        int currentHp)
    {
        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        }

        if (previousHp < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(previousHp), "Previous HP cannot be negative.");
        }

        if (currentHp < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(currentHp), "Current HP cannot be negative.");
        }

        ActorId = actorId;
        PreviousHp = previousHp;
        CurrentHp = currentHp;
    }

    public string ActorId { get; }

    public int PreviousHp { get; }

    public int CurrentHp { get; }

    public int Delta => CurrentHp - PreviousHp;
}
