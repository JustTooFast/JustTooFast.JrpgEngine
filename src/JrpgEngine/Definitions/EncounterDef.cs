// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace JustTooFast.JrpgEngine.Definitions;

public sealed class EncounterDef
{
    public string Id { get; set; } = string.Empty;

    public List<string> EnemyIds { get; set; } = new();
}