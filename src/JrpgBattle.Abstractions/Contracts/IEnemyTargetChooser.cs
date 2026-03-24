// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle.Abstractions.Contracts;

public interface IEnemyTargetChooser
{
    string ChooseTargetId(BattleState state, string actorId);
}