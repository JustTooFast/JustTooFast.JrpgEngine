// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

namespace JustTooFast.JrpgBattle.ConsoleHost;

public interface IBattleRewardApplier
{
    void Apply(BattleReward reward);
}