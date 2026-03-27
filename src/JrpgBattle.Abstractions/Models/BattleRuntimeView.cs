// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleRuntimeView
{
    public BattleRuntimeView(
        BattleState state,
        BattleInputRequest? inputRequest,
        IReadOnlyList<BattleOccurrence> occurrences,
        BattleResult? result)
    {
        State = state ?? throw new ArgumentNullException(nameof(state));
        InputRequest = inputRequest;
        Occurrences = occurrences ?? throw new ArgumentNullException(nameof(occurrences));
        Result = result;
    }

    public BattleState State { get; }

    public BattleInputRequest? InputRequest { get; }

    public IReadOnlyList<BattleOccurrence> Occurrences { get; }

    public BattleResult? Result { get; }

    public bool IsPlayerInputRequired => InputRequest is not null;

    public bool IsCompleted => Result is not null;
}