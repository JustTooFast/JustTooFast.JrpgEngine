// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleActionDefinition
{
    public BattleActionDefinition(
        BattleActionKind actionKind,
        BattleExtendedData extendedData)
    {
        if (!Enum.IsDefined(actionKind))
        {
            throw new ArgumentOutOfRangeException(nameof(actionKind), "Action kind must be a defined BattleActionKind value.");
        }

        ExtendedData = extendedData ?? throw new ArgumentNullException(nameof(extendedData));
        ActionKind = actionKind;
    }

    public BattleActionKind ActionKind { get; }

    public BattleExtendedData ExtendedData { get; }
}
