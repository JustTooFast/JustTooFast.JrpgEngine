// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgEngine.State;

public sealed class CharacterState
{
    public CharacterState(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            throw new ArgumentException("Character id cannot be null or empty.", nameof(characterId));
        }

        CharacterId = characterId;
    }

    public string CharacterId { get; }
}