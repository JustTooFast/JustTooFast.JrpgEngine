// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleInputRequest
{
    public BattleInputRequest(
        string actorId,
        BattleMenuContext currentContext,
        IEnumerable<BattleMenuContext> menuPath,
        IEnumerable<BattleMenuOption> options,
        BattleTargetSelectionState? targetSelection = null)
    {
        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        }

        ActorId = actorId;
        CurrentContext = currentContext ?? throw new ArgumentNullException(nameof(currentContext));

        if (menuPath is null)
        {
            throw new ArgumentNullException(nameof(menuPath));
        }

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        IReadOnlyList<BattleMenuContext> materializedMenuPath = menuPath.ToArray();
        IReadOnlyList<BattleMenuOption> materializedOptions = options.ToArray();

        if (materializedMenuPath.Count == 0)
        {
            throw new ArgumentException("Menu path must contain at least one context.", nameof(menuPath));
        }

        if (!materializedMenuPath.Any(c => string.Equals(c.ContextId, currentContext.ContextId, StringComparison.Ordinal)))
        {
            throw new ArgumentException("Menu path must contain the current context.", nameof(menuPath));
        }

        MenuPath = new ReadOnlyCollection<BattleMenuContext>(materializedMenuPath.ToArray());
        Options = new ReadOnlyCollection<BattleMenuOption>(materializedOptions.ToArray());
        TargetSelection = targetSelection;
    }

    public string ActorId { get; }

    public BattleMenuContext CurrentContext { get; }

    public IReadOnlyList<BattleMenuContext> MenuPath { get; }

    public IReadOnlyList<BattleMenuOption> Options { get; }

    public BattleTargetSelectionState? TargetSelection { get; }

    public bool RequiresTargetSelection => TargetSelection is not null;

    public BattleTargetMode TargetMode => TargetSelection?.TargetMode ?? BattleTargetMode.None;
}
