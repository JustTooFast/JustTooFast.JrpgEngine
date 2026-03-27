// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleFlowActorState
{
    public BattleFlowActorState(
        string actorId,
        bool isDefeated,
        bool isDefending,
        bool preventsActing)
    {
        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        }

        ActorId = actorId;
        IsDefeated = isDefeated;
        IsDefending = isDefending;
        PreventsActing = preventsActing;
    }

    public string ActorId { get; }

    public bool IsDefeated { get; }

    public bool IsDefending { get; }

    public bool PreventsActing { get; }

    public bool CanAct => !IsDefeated && !PreventsActing;
}