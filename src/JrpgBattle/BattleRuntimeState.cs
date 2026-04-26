// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

internal sealed class BattleRuntimeState
{
    private readonly IReadOnlyDictionary<string, BattleActorRuntimeState> _actorStates;
    private readonly Dictionary<string, BattleExtendedData> _actorExtendedData;

    public BattleRuntimeState(
        BattleState battleState,
        IReadOnlyDictionary<string, BattleExtendedData> actorExtendedData)
    {
        BattleState = battleState ?? throw new ArgumentNullException(nameof(battleState));

        if (actorExtendedData is null)
        {
            throw new ArgumentNullException(nameof(actorExtendedData));
        }

        Dictionary<string, BattleActorRuntimeState> actorStates = battleState.Actors
            .ToDictionary(
                static actor => actor.Id,
                static actor => new BattleActorRuntimeState(actor.Id),
                StringComparer.Ordinal);

        string[] battleActorIds = battleState.Actors
            .Select(static actor => actor.Id)
            .OrderBy(static id => id, StringComparer.Ordinal)
            .ToArray();

        string[] extendedDataActorIds = actorExtendedData.Keys
            .OrderBy(static id => id, StringComparer.Ordinal)
            .ToArray();

        if (battleActorIds.Length != extendedDataActorIds.Length ||
            !battleActorIds.SequenceEqual(extendedDataActorIds, StringComparer.Ordinal))
        {
            throw new ArgumentException(
                "Actor extended data must contain exactly one entry for each battle actor.",
                nameof(actorExtendedData));
        }

        if (actorExtendedData.Any(static kvp => kvp.Value is null))
        {
            throw new ArgumentException("Actor extended data cannot contain null values.", nameof(actorExtendedData));
        }

        _actorStates = new ReadOnlyDictionary<string, BattleActorRuntimeState>(actorStates);
        _actorExtendedData = new Dictionary<string, BattleExtendedData>(actorExtendedData, StringComparer.Ordinal);

        CurrentInputRequest = null;
        PendingPlayerChoice = null;
        CurrentOccurrences = Array.Empty<BattleOccurrence>();
        Result = null;
    }

    public BattleState BattleState { get; private set; }

    public IReadOnlyDictionary<string, BattleActorRuntimeState> ActorStates => _actorStates;

    public BattleInputRequest? CurrentInputRequest { get; private set; }

    public BattleActionChoice? PendingPlayerChoice { get; private set; }

    public IReadOnlyList<BattleOccurrence> CurrentOccurrences { get; private set; }

    public BattleResult? Result { get; private set; }

    public bool HasPendingPlayerChoice => PendingPlayerChoice is not null;

    public bool IsInputRequested => CurrentInputRequest is not null;

    public bool IsCompleted => Result is not null;

    public BattleActorRuntimeState GetActorState(string actorId)
    {
        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        }

        if (!_actorStates.TryGetValue(actorId, out BattleActorRuntimeState? actorState))
        {
            throw new InvalidOperationException($"Actor runtime state for '{actorId}' was not found.");
        }

        return actorState;
    }

    public BattleExtendedData GetActorExtendedData(string actorId)
    {
        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        }

        if (!_actorExtendedData.TryGetValue(actorId, out BattleExtendedData? extendedData))
        {
            throw new InvalidOperationException($"Actor extended data for '{actorId}' was not found.");
        }

        return extendedData;
    }

    public void SetActorExtendedData(string actorId, BattleExtendedData extendedData)
    {
        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        }

        if (extendedData is null)
        {
            throw new ArgumentNullException(nameof(extendedData));
        }

        if (!_actorExtendedData.ContainsKey(actorId))
        {
            throw new InvalidOperationException($"Actor extended data for '{actorId}' was not found.");
        }

        _actorExtendedData[actorId] = extendedData;
    }

    public void SetBattleState(BattleState state)
    {
        if (state is null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        if (state.Actors.Count != _actorStates.Count)
        {
            throw new InvalidOperationException("Battle actor count cannot change during the current runtime phase.");
        }

        foreach (BattleActorState actor in state.Actors)
        {
            if (!_actorStates.ContainsKey(actor.Id))
            {
                throw new InvalidOperationException(
                    $"Battle actor '{actor.Id}' does not have corresponding runtime-owned actor state.");
            }
        }

        BattleState = state;
    }

    public void SetInputRequest(BattleInputRequest? inputRequest)
    {
        if (inputRequest is not null)
        {
            ValidateInputRequestActor(inputRequest.ActorId);
        }

        CurrentInputRequest = inputRequest;
    }

    public void SetPendingPlayerChoice(BattleActionChoice? choice)
    {
        PendingPlayerChoice = choice;
    }

    public void SetOccurrences(IReadOnlyList<BattleOccurrence> occurrences)
    {
        if (occurrences is null)
        {
            throw new ArgumentNullException(nameof(occurrences));
        }

        if (occurrences.Any(static occurrence => occurrence is null))
        {
            throw new ArgumentException("Occurrences cannot contain null entries.", nameof(occurrences));
        }

        CurrentOccurrences = new ReadOnlyCollection<BattleOccurrence>(occurrences.ToArray());
    }

    public void ClearOccurrences()
    {
        CurrentOccurrences = Array.Empty<BattleOccurrence>();
    }

    public void SetResult(BattleResult? result)
    {
        Result = result;
    }

    private void ValidateInputRequestActor(string actorId)
    {
        BattleActorState actor = BattleState.Actors.FirstOrDefault(a => string.Equals(a.Id, actorId, StringComparison.Ordinal))
            ?? throw new InvalidOperationException(
                $"Input request actor '{actorId}' was not found in the current battle state.");

        if (!_actorStates.ContainsKey(actor.Id))
        {
            throw new InvalidOperationException(
                $"Input request actor '{actor.Id}' does not have corresponding runtime-owned actor state.");
        }

        if (actor.IsDefeated)
        {
            throw new InvalidOperationException(
                $"Cannot request input for defeated actor '{actor.Id}'.");
        }
    }
}