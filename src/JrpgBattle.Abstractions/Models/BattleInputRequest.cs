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
        IEnumerable<BattleInputAction> actions)
    {
        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        }

        if (actions is null)
        {
            throw new ArgumentNullException(nameof(actions));
        }

        BattleInputAction[] materializedActions = actions.ToArray();

        if (materializedActions.Any(static action => action is null))
        {
            throw new ArgumentException("Actions cannot contain null entries.", nameof(actions));
        }

        string[] duplicateActionIds = materializedActions
            .GroupBy(static action => action.ActionId, StringComparer.Ordinal)
            .Where(static group => group.Count() > 1)
            .Select(static group => group.Key)
            .ToArray();

        if (duplicateActionIds.Length > 0)
        {
            throw new ArgumentException("Action ids must be unique within the input request.", nameof(actions));
        }

        ActorId = actorId;
        Actions = new ReadOnlyCollection<BattleInputAction>(materializedActions);
    }

    public string ActorId { get; }

    public IReadOnlyList<BattleInputAction> Actions { get; }
}