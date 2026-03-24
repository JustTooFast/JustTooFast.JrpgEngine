// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleAdvanceResult
{
    public BattleAdvanceResult(
        bool hasChanged,
        bool isPlayerInputNeeded,
        BattleActionResult? actionResult,
        BattleResult? battleResult)
    {
        if (isPlayerInputNeeded && actionResult is not null)
        {
            throw new ArgumentException("Player input needed cannot include an action result.", nameof(actionResult));
        }

        if (isPlayerInputNeeded && battleResult is not null)
        {
            throw new ArgumentException("Player input needed cannot include a battle result.", nameof(battleResult));
        }

        if (battleResult is not null && !hasChanged)
        {
            throw new ArgumentException("A battle result implies a changed runtime state.", nameof(battleResult));
        }

        if (actionResult is not null && !hasChanged)
        {
            throw new ArgumentException("An action result implies a changed runtime state.", nameof(actionResult));
        }

        HasChanged = hasChanged;
        IsPlayerInputNeeded = isPlayerInputNeeded;
        ActionResult = actionResult;
        BattleResult = battleResult;
    }

    public bool HasChanged { get; }

    public bool IsPlayerInputNeeded { get; }

    public BattleActionResult? ActionResult { get; }

    public BattleResult? BattleResult { get; }
}