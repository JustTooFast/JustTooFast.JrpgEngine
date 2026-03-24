// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleRuntimeView
{
    public BattleRuntimeView(BattleState state)
    {
        State = state ?? throw new ArgumentNullException(nameof(state));
    }

    public BattleState State { get; }
}