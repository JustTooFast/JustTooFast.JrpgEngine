// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleActorBehaviorDefinition
{
    public BattleActorBehaviorDefinition(
        BattleBehaviorBand aggression,
        BattleBehaviorBand selfPreservation,
        BattleBehaviorBand supportiveness,
        BattleBehaviorBand opportunism,
        BattleBehaviorBand focus)
    {
        if (!Enum.IsDefined(aggression))
        {
            throw new ArgumentOutOfRangeException(nameof(aggression), "Aggression must be a defined value.");
        }

        if (!Enum.IsDefined(selfPreservation))
        {
            throw new ArgumentOutOfRangeException(nameof(selfPreservation), "Self-preservation must be a defined value.");
        }

        if (!Enum.IsDefined(supportiveness))
        {
            throw new ArgumentOutOfRangeException(nameof(supportiveness), "Supportiveness must be a defined value.");
        }

        if (!Enum.IsDefined(opportunism))
        {
            throw new ArgumentOutOfRangeException(nameof(opportunism), "Opportunism must be a defined value.");
        }

        if (!Enum.IsDefined(focus))
        {
            throw new ArgumentOutOfRangeException(nameof(focus), "Focus must be a defined value.");
        }

        Aggression = aggression;
        SelfPreservation = selfPreservation;
        Supportiveness = supportiveness;
        Opportunism = opportunism;
        Focus = focus;
    }

    public BattleBehaviorBand Aggression { get; }

    public BattleBehaviorBand SelfPreservation { get; }

    public BattleBehaviorBand Supportiveness { get; }

    public BattleBehaviorBand Opportunism { get; }

    public BattleBehaviorBand Focus { get; }
}