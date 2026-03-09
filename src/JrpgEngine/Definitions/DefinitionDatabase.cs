// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace JustTooFast.JrpgEngine.Definitions;

public sealed class DefinitionDatabase
{
    public DefinitionDatabase(
        GameConfig gameConfig,
        Dictionary<string, MapDef> maps,
        Dictionary<string, CharacterDef> characters)
    {
        GameConfig = gameConfig ?? throw new ArgumentNullException(nameof(gameConfig));
        Maps = maps ?? throw new ArgumentNullException(nameof(maps));
        Characters = characters ?? throw new ArgumentNullException(nameof(characters));
    }

    public GameConfig GameConfig { get; }

    public IReadOnlyDictionary<string, MapDef> Maps { get; }

    public IReadOnlyDictionary<string, CharacterDef> Characters { get; }
}