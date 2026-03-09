// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace JustTooFast.JrpgEngine.State;

public sealed class PartyState
{
    public PartyState(
        List<string> partyPoolCharacterIds,
        List<string> activePartyCharacterIds,
        Dictionary<string, CharacterState> characterStates)
    {
        PartyPoolCharacterIds = partyPoolCharacterIds ?? throw new ArgumentNullException(nameof(partyPoolCharacterIds));
        ActivePartyCharacterIds = activePartyCharacterIds ?? throw new ArgumentNullException(nameof(activePartyCharacterIds));
        CharacterStates = characterStates ?? throw new ArgumentNullException(nameof(characterStates));
    }

    public List<string> PartyPoolCharacterIds { get; }

    public List<string> ActivePartyCharacterIds { get; }

    public Dictionary<string, CharacterState> CharacterStates { get; }

    public string GetLeaderCharacterId()
    {
        if (ActivePartyCharacterIds.Count == 0)
        {
            throw new InvalidOperationException("Active party is empty.");
        }

        return ActivePartyCharacterIds[0];
    }
}