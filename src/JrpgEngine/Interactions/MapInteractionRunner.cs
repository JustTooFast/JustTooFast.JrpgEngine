// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using JustTooFast.JrpgEngine.Definitions;
using JustTooFast.JrpgEngine.Dialogue;
using JustTooFast.JrpgEngine.State;

namespace JustTooFast.JrpgEngine.Interactions;

public sealed class MapInteractionRunner
{
    private readonly DefinitionDatabase _definitions;
    private readonly GameState _gameState;

    public MapInteractionRunner(DefinitionDatabase definitions, GameState gameState)
    {
        _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
        _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
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
            "DialogueNpc" => StartDialogue(interaction.DialogueId),
            "Chest" => StartChest(interaction),
            _ => throw new InvalidOperationException(
                $"Unsupported interaction type '{interaction.Type}' for Slice 3.")
        };
    }

    private InteractionStartResult StartDialogue(string dialogueId)
    {
        if (string.IsNullOrWhiteSpace(dialogueId))
        {
            throw new InvalidOperationException("Dialogue interaction requires a non-empty DialogueId.");
        }

        if (!_definitions.Dialogues.TryGetValue(dialogueId, out var dialogue))
        {
            throw new InvalidOperationException(
                $"Dialogue '{dialogueId}' was not found.");
        }

        var variant = ResolveVariant(dialogue);

        return InteractionStartResult.StartDialogue(new DialogueSession(dialogue, variant));
    }

    private InteractionStartResult StartChest(InteractionDef interaction)
    {
        if (string.IsNullOrWhiteSpace(interaction.OpenFlagId))
        {
            throw new InvalidOperationException(
                $"Chest interaction '{interaction.Id}' requires OpenFlagId.");
        }

        var alreadyOpened = _gameState.StoryFlags.IsSet(interaction.OpenFlagId);

        if (alreadyOpened)
        {
            if (string.IsNullOrWhiteSpace(interaction.OpenedDialogueId))
            {
                return InteractionStartResult.None();
            }

            return StartDialogue(interaction.OpenedDialogueId);
        }

        if (string.IsNullOrWhiteSpace(interaction.DialogueId))
        {
            throw new InvalidOperationException(
                $"Chest interaction '{interaction.Id}' requires DialogueId for unopened state.");
        }

        if (string.IsNullOrWhiteSpace(interaction.ItemId))
        {
            throw new InvalidOperationException(
                $"Chest interaction '{interaction.Id}' requires ItemId.");
        }

        if (interaction.Amount <= 0)
        {
            throw new InvalidOperationException(
                $"Chest interaction '{interaction.Id}' requires Amount > 0.");
        }

        if (!_definitions.Items.TryGetValue(interaction.ItemId, out _))
        {
            throw new InvalidOperationException(
                $"Chest interaction '{interaction.Id}' references missing item '{interaction.ItemId}'.");
        }

        if (!_definitions.Dialogues.TryGetValue(interaction.DialogueId, out var baseDialogue))
        {
            throw new InvalidOperationException(
                $"Dialogue '{interaction.DialogueId}' was not found.");
        }

        var baseVariant = ResolveVariant(baseDialogue);

        var chestVariant = new DialogueVariantDef
        {
            Condition = null,
            Lines = new List<string>(baseVariant.Lines),
            Results = new List<InteractionResultDef>(baseVariant.Results)
            {
                new InteractionResultDef
                {
                    Type = "GiveItem",
                    ItemId = interaction.ItemId,
                    Amount = interaction.Amount
                },
                new InteractionResultDef
                {
                    Type = "SetFlag",
                    FlagId = interaction.OpenFlagId
                }
            }
        };

        return InteractionStartResult.StartDialogue(new DialogueSession(baseDialogue, chestVariant));
    }

    private DialogueVariantDef ResolveVariant(DialogueDef dialogue)
    {
        foreach (var variant in dialogue.Variants)
        {
            if (MatchesCondition(variant.Condition))
            {
                return variant;
            }
        }

        throw new InvalidOperationException(
            $"Dialogue '{dialogue.Id}' had no matching variant.");
    }

    private bool MatchesCondition(DialogueConditionDef? condition)
    {
        if (condition is null)
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(condition.HasFlag) &&
            !_gameState.StoryFlags.IsSet(condition.HasFlag))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(condition.LacksFlag) &&
            _gameState.StoryFlags.IsSet(condition.LacksFlag))
        {
            return false;
        }

        return true;
    }
}