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
        Dictionary<string, CharacterDef> characters,
        Dictionary<string, DialogueDef> dialogues,
        Dictionary<string, InteractionDef> interactions,
        Dictionary<string, ItemDef> items)
    {
        GameConfig = gameConfig ?? throw new ArgumentNullException(nameof(gameConfig));
        Maps = maps ?? throw new ArgumentNullException(nameof(maps));
        Characters = characters ?? throw new ArgumentNullException(nameof(characters));
        Dialogues = dialogues ?? throw new ArgumentNullException(nameof(dialogues));
        Interactions = interactions ?? throw new ArgumentNullException(nameof(interactions));
        Items = items ?? throw new ArgumentNullException(nameof(items));
    }

    public GameConfig GameConfig { get; }

    public IReadOnlyDictionary<string, MapDef> Maps { get; }

    public IReadOnlyDictionary<string, CharacterDef> Characters { get; }

    public IReadOnlyDictionary<string, DialogueDef> Dialogues { get; }

    public IReadOnlyDictionary<string, InteractionDef> Interactions { get; }

    public IReadOnlyDictionary<string, ItemDef> Items { get; }
}