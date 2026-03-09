// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using JustTooFast.JrpgEngine.Definitions;
using JustTooFast.JrpgEngine.State;

namespace JustTooFast.JrpgEngine.Systems;

public sealed class RuntimeStateValidator
{
    public void Validate(GameState gameState, DefinitionDatabase definitions)
    {
        if (gameState is null)
        {
            throw new ArgumentNullException(nameof(gameState));
        }

        if (definitions is null)
        {
            throw new ArgumentNullException(nameof(definitions));
        }

        ValidateCurrentMap(gameState, definitions);
        ValidatePlayerPosition(gameState, definitions);
        ValidateParty(gameState, definitions);
    }

    private static void ValidateCurrentMap(GameState gameState, DefinitionDatabase definitions)
    {
        if (!definitions.Maps.ContainsKey(gameState.CurrentMapId))
        {
            throw new InvalidOperationException(
                $"GameState.CurrentMapId '{gameState.CurrentMapId}' does not exist.");
        }
    }

    private static void ValidatePlayerPosition(GameState gameState, DefinitionDatabase definitions)
    {
        var map = definitions.Maps[gameState.CurrentMapId];

        if (gameState.PlayerTileX < 0 || gameState.PlayerTileX >= map.Width ||
            gameState.PlayerTileY < 0 || gameState.PlayerTileY >= map.Height)
        {
            throw new InvalidOperationException(
                $"Player tile ({gameState.PlayerTileX}, {gameState.PlayerTileY}) is out of bounds for map '{map.Id}'.");
        }

        foreach (var blockedTile in map.BlockedTiles)
        {
            if (blockedTile.X == gameState.PlayerTileX &&
                blockedTile.Y == gameState.PlayerTileY)
            {
                throw new InvalidOperationException(
                    $"Player tile ({gameState.PlayerTileX}, {gameState.PlayerTileY}) is blocked on map '{map.Id}'.");
            }
        }
    }

    private static void ValidateParty(GameState gameState, DefinitionDatabase definitions)
    {
        var party = gameState.Party;

        if (party.ActivePartyCharacterIds.Count == 0)
        {
            throw new InvalidOperationException("Active party must contain at least one character.");
        }

        if (party.ActivePartyCharacterIds.Count > 5)
        {
            throw new InvalidOperationException("Active party cannot contain more than 5 characters.");
        }

        var poolSet = new HashSet<string>(party.PartyPoolCharacterIds, StringComparer.Ordinal);

        foreach (var activeCharacterId in party.ActivePartyCharacterIds)
        {
            if (!poolSet.Contains(activeCharacterId))
            {
                throw new InvalidOperationException(
                    $"Active party character '{activeCharacterId}' is not in the party pool.");
            }
        }

        foreach (var poolCharacterId in party.PartyPoolCharacterIds)
        {
            if (!definitions.Characters.ContainsKey(poolCharacterId))
            {
                throw new InvalidOperationException(
                    $"Party pool references missing character definition '{poolCharacterId}'.");
            }

            if (!party.CharacterStates.ContainsKey(poolCharacterId))
            {
                throw new InvalidOperationException(
                    $"Party pool character '{poolCharacterId}' is missing runtime CharacterState.");
            }
        }

        foreach (var (characterId, characterState) in party.CharacterStates)
        {
            if (!definitions.Characters.ContainsKey(characterId))
            {
                throw new InvalidOperationException(
                    $"Runtime CharacterState references missing character definition '{characterId}'.");
            }

            if (!string.Equals(characterState.CharacterId, characterId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"CharacterState key '{characterId}' does not match CharacterState.CharacterId '{characterState.CharacterId}'.");
            }
        }
    }
}