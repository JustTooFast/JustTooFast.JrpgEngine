// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattlePresentationView
{
    public BattlePresentationView(BattleRuntimeView runtimeView)
    {
        RuntimeView = runtimeView ?? throw new ArgumentNullException(nameof(runtimeView));
    }

    public BattleRuntimeView RuntimeView { get; }
}