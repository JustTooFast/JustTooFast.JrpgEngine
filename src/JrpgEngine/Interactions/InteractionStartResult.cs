// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.Dialogue;

namespace JustTooFast.JrpgEngine.Interactions;

public sealed class InteractionStartResult
{
    private InteractionStartResult(
        bool started,
        DialogueSession? dialogueSession,
        PendingMapTransitionRequest? pendingMapTransition)
    {
        if (started && dialogueSession is null && pendingMapTransition is null)
        {
            throw new InvalidOperationException(
                "A started interaction must produce either dialogue or a map transition.");
        }

        if (dialogueSession is not null && pendingMapTransition is not null)
        {
            throw new InvalidOperationException(
                "An interaction cannot start both dialogue and a map transition at the same time.");
        }

        Started = started;
        DialogueSession = dialogueSession;
        PendingMapTransition = pendingMapTransition;
    }

    public bool Started { get; }

    public DialogueSession? DialogueSession { get; }

    public PendingMapTransitionRequest? PendingMapTransition { get; }

    public static InteractionStartResult None() => new(false, null, null);

    public static InteractionStartResult StartDialogue(DialogueSession session)
    {
        if (session is null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        return new(true, session, null);
    }

    public static InteractionStartResult StartMapTransition(
        string destinationMapId,
        string destinationSpawnId)
    {
        if (string.IsNullOrWhiteSpace(destinationMapId))
        {
            throw new ArgumentException(
                "Destination map id cannot be null or empty.",
                nameof(destinationMapId));
        }

        if (string.IsNullOrWhiteSpace(destinationSpawnId))
        {
            throw new ArgumentException(
                "Destination spawn id cannot be null or empty.",
                nameof(destinationSpawnId));
        }

        return new(
            true,
            null,
            new PendingMapTransitionRequest(destinationMapId, destinationSpawnId));
    }
}

public sealed class PendingMapTransitionRequest
{
    public PendingMapTransitionRequest(string destinationMapId, string destinationSpawnId)
    {
        DestinationMapId = destinationMapId;
        DestinationSpawnId = destinationSpawnId;
    }

    public string DestinationMapId { get; }

    public string DestinationSpawnId { get; }
}