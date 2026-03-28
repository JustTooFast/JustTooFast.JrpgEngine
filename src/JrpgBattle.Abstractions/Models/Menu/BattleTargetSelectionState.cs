// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleTargetSelectionState
{
    public BattleTargetSelectionState(
        BattleTargetMode targetMode,
        IEnumerable<BattleSelectableTarget> selectableTargets)
    {
        if (targetMode == BattleTargetMode.None)
        {
            throw new ArgumentException("Target selection state cannot use target mode None.", nameof(targetMode));
        }

        if (selectableTargets is null)
        {
            throw new ArgumentNullException(nameof(selectableTargets));
        }

        IReadOnlyList<BattleSelectableTarget> materializedTargets = selectableTargets.ToArray();

        if (materializedTargets.Count == 0)
        {
            throw new ArgumentException("Target selection must contain at least one target.", nameof(selectableTargets));
        }

        TargetMode = targetMode;
        SelectableTargets = new ReadOnlyCollection<BattleSelectableTarget>(materializedTargets.ToArray());
    }

    public BattleTargetMode TargetMode { get; }

    public IReadOnlyList<BattleSelectableTarget> SelectableTargets { get; }
}
