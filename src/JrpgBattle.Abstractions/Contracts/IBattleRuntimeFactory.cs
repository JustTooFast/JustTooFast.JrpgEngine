// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

namespace JustTooFast.JrpgBattle.Abstractions.Contracts;

public interface IBattleRuntimeFactory
{
    IBattleRuntime Create();
}