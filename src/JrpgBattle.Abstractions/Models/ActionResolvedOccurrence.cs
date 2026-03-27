// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record ActionResolvedOccurrence : BattleOccurrence
{
    public ActionResolvedOccurrence(
        string actorId,
        BattleActionKind actionKind,
        string? actionId)
    {
        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        }

        ActorId = actorId;
        ActionKind = actionKind;
        ActionId = string.IsNullOrWhiteSpace(actionId) ? null : actionId;
    }

    public string ActorId { get; }

    public BattleActionKind ActionKind { get; }

    public string? ActionId { get; }
}
