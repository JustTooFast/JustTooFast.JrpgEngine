// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record ConditionRemovedOccurrence : BattleOccurrence
{
    public ConditionRemovedOccurrence(string actorId, string conditionId)
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
    }

    public string ActorId { get; }

    public string ConditionId { get; }
}