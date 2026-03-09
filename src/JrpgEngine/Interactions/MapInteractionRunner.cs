// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.Definitions;
using JustTooFast.JrpgEngine.Dialogue;

namespace JustTooFast.JrpgEngine.Interactions;

public sealed class MapInteractionRunner
{
    private readonly DefinitionDatabase _definitions;

    public MapInteractionRunner(DefinitionDatabase definitions)
    {
        _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
    }

    public InteractionStartResult TryStart(string interactionId)
    {
        if (string.IsNullOrWhiteSpace(interactionId))
        {
            throw new ArgumentException("Interaction id cannot be null or empty.", nameof(interactionId));
        }

        if (!_definitions.Interactions.TryGetValue(interactionId, out var interaction))
        {
            throw new InvalidOperationException(
                $"Interaction '{interactionId}' was not found.");
        }

        return interaction.Type switch
        {
            "DialogueNpc" => StartDialogue(interaction),
            _ => throw new InvalidOperationException(
                $"Unsupported interaction type '{interaction.Type}' for Slice 2.")
        };
    }

    private InteractionStartResult StartDialogue(InteractionDef interaction)
    {
        if (!_definitions.Dialogues.TryGetValue(interaction.DialogueId, out var dialogue))
        {
            throw new InvalidOperationException(
                $"Dialogue '{interaction.DialogueId}' was not found.");
        }

        return InteractionStartResult.StartDialogue(new DialogueSession(dialogue));
    }
}