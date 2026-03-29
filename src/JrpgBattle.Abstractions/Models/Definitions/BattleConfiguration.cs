// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleConfiguration
{
    public BattleConfiguration(
        BattleTeam startingTeam,
        BattleOpeningAdvantage openingAdvantage,
        bool canEscape,
        BattleExtendedData extendedData)
    {
        if (startingTeam is not BattleTeam.Party and not BattleTeam.Enemy)
        {
            throw new ArgumentOutOfRangeException(nameof(startingTeam), "Starting team must be Party or Enemy.");
        }

        if (!Enum.IsDefined(openingAdvantage))
        {
            throw new ArgumentOutOfRangeException(nameof(openingAdvantage), "Opening advantage must be a defined value.");
        }

        StartingTeam = startingTeam;
        OpeningAdvantage = openingAdvantage;
        CanEscape = canEscape;
        ExtendedData = extendedData ?? throw new ArgumentNullException(nameof(extendedData));
    }

    public BattleTeam StartingTeam { get; }

    public BattleOpeningAdvantage OpeningAdvantage { get; }

    public bool CanEscape { get; }

    public BattleExtendedData ExtendedData { get; }
}