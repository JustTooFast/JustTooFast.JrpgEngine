// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.ConsoleHost;

public sealed record BattleReward
{
    public BattleReward(int experiencePoints)
    {
        if (experiencePoints < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(experiencePoints), "Experience points cannot be negative.");
        }

        ExperiencePoints = experiencePoints;
    }

    public int ExperiencePoints { get; }
}