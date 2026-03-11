// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace JustTooFast.JrpgEngine.Definitions;

public sealed class EncounterTableEntryDef
{
    public string EncounterId { get; set; } = string.Empty;

    public int Weight { get; set; }
}