// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleAbilityDefinition
{
    public BattleAbilityDefinition(
        string id,
        string name,
        BattleTargetMode targetMode,
        BattleExtendedData extendedData)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Spell id is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Spell name is required.", nameof(name));
        }

        if (!Enum.IsDefined(targetMode))
        {
            throw new ArgumentOutOfRangeException(nameof(targetMode), "Target mode must be a defined BattleTargetMode value.");
        }

        if (extendedData is null)
        {
            throw new ArgumentNullException(nameof(extendedData));
        }

        Id = id;
        Name = name;
        TargetMode = targetMode;
        ExtendedData = extendedData;
    }

    public string Id { get; }

    public string Name { get; }

    public BattleTargetMode TargetMode { get; }

    public BattleExtendedData ExtendedData { get; }
}