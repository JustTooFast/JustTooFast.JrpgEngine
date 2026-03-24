// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

namespace JustTooFast.JrpgBattle;

internal enum BattleRuntimePhase
{
    Advancing = 0,
    WaitingForPlayerChoice = 1,
    Ended = 2,
}