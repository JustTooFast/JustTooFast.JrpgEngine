// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

namespace JustTooFast.JrpgEngine.Definitions;

public sealed class InteractionDef
{
    public string Id { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string DialogueId { get; set; } = string.Empty;

    public string OpenedDialogueId { get; set; } = string.Empty;

    public string ItemId { get; set; } = string.Empty;

    public int Amount { get; set; } = 1;

    public string OpenFlagId { get; set; } = string.Empty;

    public string RequiredFlagId { get; set; } = string.Empty;

    public string DestinationMapId { get; set; } = string.Empty;

    public string DestinationSpawnId { get; set; } = string.Empty;

    public bool TriggerOnStep { get; set; }
}