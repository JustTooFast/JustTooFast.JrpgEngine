// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleRuntimeView
{
    public BattleRuntimeView(
        BattleState state,
        string? pendingPlayerActorId)
    {
        State = state ?? throw new ArgumentNullException(nameof(state));
        PendingPlayerActorId = pendingPlayerActorId;
    }

    public BattleState State { get; }

    public string? PendingPlayerActorId { get; }
}