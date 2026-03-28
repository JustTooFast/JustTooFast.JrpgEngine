using System;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleSelectableTarget
{
    public BattleSelectableTarget(
        string actorId,
        string label,
        bool isEnabled)
    {
        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        }

        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Target label is required.", nameof(label));
        }

        ActorId = actorId;
        Label = label;
        IsEnabled = isEnabled;
    }

    public string ActorId { get; }

    public string Label { get; }

    public bool IsEnabled { get; }
}
