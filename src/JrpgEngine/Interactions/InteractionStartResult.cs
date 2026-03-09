// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.Dialogue;

namespace JustTooFast.JrpgEngine.Interactions;

public sealed class InteractionStartResult
{
    private InteractionStartResult(bool started, DialogueSession? dialogueSession)
    {
        if (started && dialogueSession is null)
        {
            throw new ArgumentNullException(nameof(dialogueSession));
        }

        Started = started;
        DialogueSession = dialogueSession;
    }

    public bool Started { get; }

    public DialogueSession? DialogueSession { get; }

    public static InteractionStartResult None() => new(false, null);

    public static InteractionStartResult StartDialogue(DialogueSession session) =>
        new(true, session);
}