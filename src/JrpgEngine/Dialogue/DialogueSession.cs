// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.Definitions;

namespace JustTooFast.JrpgEngine.Dialogue;

public sealed class DialogueSession
{
    private readonly DialogueDef _definition;
    private int _currentLineIndex;

    public DialogueSession(DialogueDef definition)
    {
        _definition = definition ?? throw new ArgumentNullException(nameof(definition));

        if (_definition.Lines.Count == 0)
        {
            throw new InvalidOperationException(
                $"Dialogue '{_definition.Id}' must have at least one line.");
        }

        _currentLineIndex = 0;
    }

    public string CurrentLine => _definition.Lines[_currentLineIndex];

    public bool Advance()
    {
        if (_currentLineIndex < _definition.Lines.Count - 1)
        {
            _currentLineIndex++;
            return false;
        }

        return true;
    }
}