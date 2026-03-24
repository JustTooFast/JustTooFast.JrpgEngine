// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

internal sealed class BattleRuntimeState
{
    public BattleRuntimeState(BattleState battleState)
    {
        BattleState = battleState ?? throw new ArgumentNullException(nameof(battleState));
        Phase = BattleRuntimePhase.Advancing;
        PendingActorId = null;
    }

    public BattleState BattleState { get; private set; }

    public BattleRuntimePhase Phase { get; private set; }

    public string? PendingActorId { get; private set; }

    public void SetBattleState(BattleState state)
    {
        BattleState = state ?? throw new ArgumentNullException(nameof(state));
    }

    public void SetPhase(BattleRuntimePhase phase)
    {
        Phase = phase;
    }

    public void SetPendingActor(string? actorId)
    {
        PendingActorId = actorId;
    }
}