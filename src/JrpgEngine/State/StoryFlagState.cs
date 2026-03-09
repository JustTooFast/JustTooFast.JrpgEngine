// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace JustTooFast.JrpgEngine.State;

public sealed class StoryFlagState
{
    private readonly HashSet<string> _flags = new(StringComparer.Ordinal);

    public bool IsSet(string flagId)
    {
        if (string.IsNullOrWhiteSpace(flagId))
        {
            throw new ArgumentException("Flag id cannot be null or empty.", nameof(flagId));
        }

        return _flags.Contains(flagId);
    }

    public void Set(string flagId)
    {
        if (string.IsNullOrWhiteSpace(flagId))
        {
            throw new ArgumentException("Flag id cannot be null or empty.", nameof(flagId));
        }

        _flags.Add(flagId);
    }
}