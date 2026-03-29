// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleExtendedData
{
    public static BattleExtendedData Empty { get; } = new(
        Array.Empty<BattleExtendedDataEntry>());

    public BattleExtendedData(
        IReadOnlyList<BattleExtendedDataEntry> entries)
    {
        if (entries is null)
        {
            throw new ArgumentNullException(nameof(entries));
        }

        if (entries.Any(static e => e is null))
        {
            throw new ArgumentException("Extended data entries cannot contain null entries.", nameof(entries));
        }

        string[] duplicateKeys = entries
            .GroupBy(static e => e.Key, StringComparer.Ordinal)
            .Where(static g => g.Count() > 1)
            .Select(static g => g.Key)
            .ToArray();

        if (duplicateKeys.Length > 0)
        {
            throw new ArgumentException(
                $"Extended data keys must be unique: {string.Join(", ", duplicateKeys)}.",
                nameof(entries));
        }

        Entries = new ReadOnlyCollection<BattleExtendedDataEntry>(entries.ToArray());
    }

    public IReadOnlyList<BattleExtendedDataEntry> Entries { get; }

    public bool ContainsKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Extended data key is required.", nameof(key));
        }

        return Entries.Any(e => string.Equals(e.Key, key, StringComparison.Ordinal));
    }

    public bool TryGetValue(string key, out string? value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Extended data key is required.", nameof(key));
        }

        BattleExtendedDataEntry? entry = Entries.FirstOrDefault(
            e => string.Equals(e.Key, key, StringComparison.Ordinal));

        if (entry is null)
        {
            value = null;
            return false;
        }

        value = entry.Value;
        return true;
    }

    public string? GetValueOrDefault(string key)
    {
        return TryGetValue(key, out string? value)
            ? value
            : null;
    }
}