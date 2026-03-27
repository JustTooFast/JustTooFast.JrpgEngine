// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

internal sealed class ConditionInstance
{
    public ConditionInstance(
        string actorId,
        string conditionId,
        ConditionDurationBand durationBand,
        bool preventsActing,
        bool clearedByDamage,
        bool persistsAfterBattle)
    {
        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        }

        if (string.IsNullOrWhiteSpace(conditionId))
        {
            throw new ArgumentException("Condition id is required.", nameof(conditionId));
        }

        ActorId = actorId;
        ConditionId = conditionId;
        DurationBand = durationBand;
        PreventsActing = preventsActing;
        ClearedByDamage = clearedByDamage;
        PersistsAfterBattle = persistsAfterBattle;
    }

    public string ActorId { get; }

    public string ConditionId { get; }

    public ConditionDurationBand DurationBand { get; }

    public bool PreventsActing { get; }

    public bool ClearedByDamage { get; }

    public bool PersistsAfterBattle { get; }

    public bool IsExpired { get; private set; }

    public void Expire()
    {
        IsExpired = true;
    }
}