// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleExtendedDataEntry
{
    public BattleExtendedDataEntry(
        string key,
        string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Extended data key is required.", nameof(key));
        }

        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        Key = key;
        Value = value;
    }

    public string Key { get; }

    public string Value { get; }
}