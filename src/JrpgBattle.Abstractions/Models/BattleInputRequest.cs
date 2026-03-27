// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleInputRequest
{
    public BattleInputRequest(string actorId)
    {
        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        }

        ActorId = actorId;
    }

    public string ActorId { get; }
}