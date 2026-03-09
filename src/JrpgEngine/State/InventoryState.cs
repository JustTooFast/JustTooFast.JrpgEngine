// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace JustTooFast.JrpgEngine.State;

public sealed class InventoryState
{
    private readonly Dictionary<string, int> _items = new(StringComparer.Ordinal);

    public void AddItem(string itemId, int amount)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            throw new ArgumentException("Item id cannot be null or empty.", nameof(itemId));
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
        }

        if (_items.TryGetValue(itemId, out var count))
        {
            _items[itemId] = count + amount;
        }
        else
        {
            _items[itemId] = amount;
        }
    }

    public bool HasItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            throw new ArgumentException("Item id cannot be null or empty.", nameof(itemId));
        }

        return _items.TryGetValue(itemId, out var count) && count > 0;
    }

    public int GetCount(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            throw new ArgumentException("Item id cannot be null or empty.", nameof(itemId));
        }

        return _items.TryGetValue(itemId, out var count) ? count : 0;
    }
}