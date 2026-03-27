// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record DamageOperation : BattleOperation
{
    public DamageOperation(string targetId, int amount)
    {
        if (string.IsNullOrWhiteSpace(targetId))
        {
            throw new ArgumentException("Target id is required.", nameof(targetId));
        }

        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Damage amount cannot be negative.");
        }

        TargetId = targetId;
        Amount = amount;
    }

    public string TargetId { get; }

    public int Amount { get; }
}
