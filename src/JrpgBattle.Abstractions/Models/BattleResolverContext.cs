// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleResolverContext
{
    public BattleResolverContext(
        BattleDefinition definition,
        BattleState state,
        string actorId,
        BattleActionChoice action)
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

        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        Definition = definition;
        State = state;
        ActorId = actorId;
        Action = action;
    }

    public BattleDefinition Definition { get; }

    public BattleState State { get; }

    public string ActorId { get; }

    public BattleActionChoice Action { get; }
}