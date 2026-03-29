// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleChooserContext
{
    public BattleChooserContext(
        BattleDefinition definition,
        BattleState state,
        string actorId)
    {
        if (definition is null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        if (state is null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        }

        Definition = definition;
        State = state;
        ActorId = actorId;
    }

    public BattleDefinition Definition { get; }

    public BattleState State { get; }

    public string ActorId { get; }
}