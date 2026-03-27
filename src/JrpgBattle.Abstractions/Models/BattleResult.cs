// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleResult
{
    public BattleResult(BattleOutcome outcome)
    {
        if (outcome == BattleOutcome.None)
        {
            throw new ArgumentException("Battle result requires a final outcome.", nameof(outcome));
        }

        Outcome = outcome;
    }

    public BattleOutcome Outcome { get; }
}