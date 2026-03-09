// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace JustTooFast.JrpgEngine.Definitions;

public sealed class DialogueVariantDef
{
    public DialogueConditionDef? Condition { get; set; }

    public List<string> Lines { get; set; } = new();

    public List<InteractionResultDef> Results { get; set; } = new();
}