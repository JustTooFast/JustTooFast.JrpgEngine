// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle.ConsoleHost;

public sealed class FixedXpBattleRewardCalculator : IBattleRewardCalculator
{
    private readonly int _experiencePoints;

    public FixedXpBattleRewardCalculator(int experiencePoints)
    {
        if (experiencePoints < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(experiencePoints), "Experience points cannot be negative.");
        }

        _experiencePoints = experiencePoints;
    }

    public BattleReward Calculate(BattleState finalState)
    {
        if (finalState is null)
        {
            throw new ArgumentNullException(nameof(finalState));
        }

        return new BattleReward(_experiencePoints);
    }
}