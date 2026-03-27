// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleResolution
{
    public BattleResolution(IReadOnlyList<BattleOperation> operations)
    {
        Operations = operations ?? throw new ArgumentNullException(nameof(operations));
    }

    public IReadOnlyList<BattleOperation> Operations { get; }
}