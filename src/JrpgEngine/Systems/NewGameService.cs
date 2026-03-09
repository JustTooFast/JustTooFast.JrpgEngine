// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using JustTooFast.JrpgEngine.Definitions;
using JustTooFast.JrpgEngine.State;

namespace JustTooFast.JrpgEngine.Systems;

public sealed class NewGameService
{
    private readonly RuntimeStateValidator _runtimeStateValidator;

    public NewGameService(RuntimeStateValidator runtimeStateValidator)
    {
        _runtimeStateValidator = runtimeStateValidator
            ?? throw new ArgumentNullException(nameof(runtimeStateValidator));
    }

    public GameState CreateNewGame(DefinitionDatabase definitions)
    {
        if (definitions is null)
        {
            throw new ArgumentNullException(nameof(definitions));
        }

        var config = definitions.GameConfig;

        var facing = ParseFacingDirection(config.StartingFacing);

        var partyPoolCharacterIds = new List<string>(config.StartingPartyMemberIds);
        var activePartyCharacterIds = new List<string>(config.StartingPartyMemberIds);

        var characterStates = new Dictionary<string, CharacterState>(StringComparer.Ordinal);
        foreach (var characterId in partyPoolCharacterIds)
        {
            characterStates.Add(characterId, new CharacterState(characterId));
        }

        var partyState = new PartyState(
            partyPoolCharacterIds,
            activePartyCharacterIds,
            characterStates);

        var gameState = new GameState(
            currentMapId: config.StartingMapId,
            playerTileX: config.StartingPlayerTileX,
            playerTileY: config.StartingPlayerTileY,
            facing: facing,
            party: partyState);

        _runtimeStateValidator.Validate(gameState, definitions);

        return gameState;
    }

    private static FacingDirection ParseFacingDirection(string facing)
    {
        if (Enum.TryParse<FacingDirection>(facing, ignoreCase: true, out var result))
        {
            return result;
        }

        throw new InvalidOperationException(
            $"Unsupported starting facing '{facing}'. Expected Up, Down, Left, or Right.");
    }
}