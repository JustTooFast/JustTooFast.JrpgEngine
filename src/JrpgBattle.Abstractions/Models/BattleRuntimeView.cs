// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleRuntimeView
{
    public BattleRuntimeView(
        BattleState battleState,
        BattleInputRequest? inputRequest,
        IReadOnlyList<BattleOccurrence> occurrences,
        BattleResult? result)
    {
        if (battleState is null)
        {
            throw new ArgumentNullException(nameof(battleState));
        }

        if (occurrences is null)
        {
            throw new ArgumentNullException(nameof(occurrences));
        }

        if (occurrences.Any(static o => o is null))
        {
            throw new ArgumentException("Occurrences cannot contain null entries.", nameof(occurrences));
        }

        BattleState = battleState;
        InputRequest = inputRequest;
        Occurrences = new ReadOnlyCollection<BattleOccurrence>(occurrences.ToArray());
        Result = result;
    }

    public BattleState BattleState { get; }

    public BattleInputRequest? InputRequest { get; }

    public IReadOnlyList<BattleOccurrence> Occurrences { get; }

    public BattleResult? Result { get; }

    public bool IsPlayerInputRequired => InputRequest is not null;

    public bool IsCompleted => Result is not null;
}