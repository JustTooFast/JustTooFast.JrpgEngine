// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleFlowStep
{
    public BattleFlowStep(
        bool hasAdvanced,
        string? readyActorId)
    {
        HasAdvanced = hasAdvanced;
        ReadyActorId = readyActorId;
    }

    public bool HasAdvanced { get; }

    public string? ReadyActorId { get; }
}