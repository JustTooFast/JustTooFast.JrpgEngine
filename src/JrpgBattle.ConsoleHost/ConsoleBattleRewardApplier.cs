// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle.ConsoleHost;

public sealed class ConsoleBattleRewardApplier : IBattleRewardApplier
{
    public void Apply(BattleReward reward)
    {
        if (reward is null)
        {
            throw new ArgumentNullException(nameof(reward));
        }

        Console.WriteLine($"Applied reward: {reward.ExperiencePoints} XP");
    }
}