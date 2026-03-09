// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using JustTooFast.JrpgEngine.Definitions;

namespace JustTooFast.JrpgEngine.Dialogue;

public sealed class DialogueSession
{
    private readonly DialogueDef _definition;
    private readonly DialogueVariantDef _variant;
    private int _currentLineIndex;

    public DialogueSession(DialogueDef definition, DialogueVariantDef variant)
    {
        _definition = definition ?? throw new ArgumentNullException(nameof(definition));
        _variant = variant ?? throw new ArgumentNullException(nameof(variant));

        if (_variant.Lines.Count == 0)
        {
            throw new InvalidOperationException(
                $"Dialogue '{_definition.Id}' variant must have at least one line.");
        }

        _currentLineIndex = 0;
    }

    public string CurrentLine => _variant.Lines[_currentLineIndex];

    public IReadOnlyList<InteractionResultDef> Results => _variant.Results;

    public bool Advance()
    {
        if (_currentLineIndex < _variant.Lines.Count - 1)
        {
            _currentLineIndex++;
            return false;
        }

        return true;
    }
}