// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle.Abstractions.Contracts;

public interface IBattleRuntime
{
    BattleState Initialize(BattleDefinition definition);

    BattleState ApplyAction(BattleState state, BattleActionChoice action);

    BattleResult GetResult(BattleState state);
}