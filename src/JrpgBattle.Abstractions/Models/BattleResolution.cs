// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleResolution
{
    public BattleResolution(IReadOnlyList<BattleOperation> operations)
    {
        if (operations is null)
        {
            throw new ArgumentNullException(nameof(operations));
        }

        if (operations.Any(static o => o is null))
        {
            throw new ArgumentException("Operations cannot contain null entries.", nameof(operations));
        }

        Operations = new ReadOnlyCollection<BattleOperation>(operations.ToArray());
    }

    public IReadOnlyList<BattleOperation> Operations { get; }
}