// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle.Abstractions.Contracts;

public interface IBattleRuntime
{
    BattleRuntimeView GetView();

    void Advance();

    void SubmitPlayerChoice(BattleActionChoice choice);
}