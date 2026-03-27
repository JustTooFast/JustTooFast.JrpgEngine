// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleFlowState
{
    public BattleFlowState(
        BattleState battleState,
        IReadOnlyDictionary<string, BattleFlowActorState> actorStates)
    {
        BattleState = battleState ?? throw new ArgumentNullException(nameof(battleState));

        if (actorStates is null)
        {
            throw new ArgumentNullException(nameof(actorStates));
        }

        string[] actorIds = battleState.Actors
            .Select(static a => a.Id)
            .OrderBy(static id => id, StringComparer.Ordinal)
            .ToArray();

        string[] flowActorIds = actorStates.Keys
            .OrderBy(static id => id, StringComparer.Ordinal)
            .ToArray();

        if (actorIds.Length != flowActorIds.Length || !actorIds.SequenceEqual(flowActorIds, StringComparer.Ordinal))
        {
            throw new ArgumentException(
                "Flow actor states must contain exactly one entry for each battle actor.",
                nameof(actorStates));
        }

        if (actorStates.Any(static kvp => kvp.Value is null))
        {
            throw new ArgumentException("Flow actor states cannot contain null values.", nameof(actorStates));
        }

        foreach ((string actorId, BattleFlowActorState actorState) in actorStates)
        {
            if (!string.Equals(actorId, actorState.ActorId, StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    $"Flow actor state key '{actorId}' does not match actor state actor id '{actorState.ActorId}'.",
                    nameof(actorStates));
            }
        }

        ActorStates = new ReadOnlyDictionary<string, BattleFlowActorState>(
            new Dictionary<string, BattleFlowActorState>(actorStates, StringComparer.Ordinal));
    }

    public BattleState BattleState { get; }

    public IReadOnlyDictionary<string, BattleFlowActorState> ActorStates { get; }

    public BattleFlowActorState GetActorState(string actorId)
    {
        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        }

        if (!ActorStates.TryGetValue(actorId, out BattleFlowActorState? actorState))
        {
            throw new InvalidOperationException($"Flow actor state for '{actorId}' was not found.");
        }

        return actorState;
    }
}