// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleMenuContext
{
    public BattleMenuContext(
        string contextId,
        BattleMenuContextKind kind,
        string? title = null,
        string? parentContextId = null,
        BattleActionKind? actionKind = null,
        string? actionId = null)
    {
        if (string.IsNullOrWhiteSpace(contextId))
        {
            throw new ArgumentException("Context id is required.", nameof(contextId));
        }

        ContextId = contextId;
        Kind = kind;
        Title = title;
        ParentContextId = parentContextId;
        ActionKind = actionKind;
        ActionId = actionId;
    }

    public string ContextId { get; }

    public BattleMenuContextKind Kind { get; }

    public string? Title { get; }

    public string? ParentContextId { get; }

    public BattleActionKind? ActionKind { get; }

    public string? ActionId { get; }
}
