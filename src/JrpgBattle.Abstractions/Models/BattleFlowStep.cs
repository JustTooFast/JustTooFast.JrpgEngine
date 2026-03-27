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
        if (hasAdvanced && string.IsNullOrWhiteSpace(readyActorId))
        {
            throw new ArgumentException("A ready actor id is required when flow has advanced.", nameof(readyActorId));
        }

        if (!hasAdvanced && readyActorId is not null)
        {
            throw new ArgumentException("Ready actor id must be null when flow has not advanced.", nameof(readyActorId));
        }

        HasAdvanced = hasAdvanced;
        ReadyActorId = readyActorId;
    }

    public bool HasAdvanced { get; }

    public string? ReadyActorId { get; }
}