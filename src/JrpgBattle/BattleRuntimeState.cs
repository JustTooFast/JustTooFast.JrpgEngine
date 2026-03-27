// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

internal sealed class BattleRuntimeState
{
    public BattleRuntimeState(BattleState battleState)
    {
        BattleState = battleState ?? throw new ArgumentNullException(nameof(battleState));
        CurrentInputRequest = null;
        PendingPlayerChoice = null;
        CurrentOccurrences = Array.Empty<BattleOccurrence>();
        Result = null;
    }

    public BattleState BattleState { get; private set; }

    public BattleInputRequest? CurrentInputRequest { get; private set; }

    public BattleActionChoice? PendingPlayerChoice { get; private set; }

    public IReadOnlyList<BattleOccurrence> CurrentOccurrences { get; private set; }

    public BattleResult? Result { get; private set; }

    public void SetBattleState(BattleState state)
    {
        BattleState = state ?? throw new ArgumentNullException(nameof(state));
    }

    public void SetInputRequest(BattleInputRequest? inputRequest)
    {
        CurrentInputRequest = inputRequest;
    }

    public void SetPendingPlayerChoice(BattleActionChoice? choice)
    {
        PendingPlayerChoice = choice;
    }

    public void SetOccurrences(IReadOnlyList<BattleOccurrence> occurrences)
    {
        CurrentOccurrences = occurrences ?? throw new ArgumentNullException(nameof(occurrences));
    }

    public void SetResult(BattleResult? result)
    {
        Result = result;
    }
}