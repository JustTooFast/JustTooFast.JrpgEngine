// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JustTooFast.JrpgBattle;

internal sealed class BattleActorRuntimeState
{
    private readonly List<ConditionInstance> _conditions;

    public BattleActorRuntimeState(string actorId)
    {
        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        }

        ActorId = actorId;
        _conditions = new List<ConditionInstance>();
        Conditions = new ReadOnlyCollection<ConditionInstance>(_conditions);
    }

    public string ActorId { get; }

    public IReadOnlyList<ConditionInstance> Conditions { get; }

    public bool IsDefending { get; private set; }

    public bool PreventedFromActing => _conditions.Any(static c => c.PreventsActing && !c.IsExpired);

    public void AddCondition(ConditionInstance instance)
    {
        if (instance is null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        if (!string.Equals(instance.ActorId, ActorId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Condition '{instance.ConditionId}' belongs to actor '{instance.ActorId}', not '{ActorId}'.");
        }

        _conditions.Add(instance);
    }

    public void RemoveCondition(ConditionInstance instance)
    {
        if (instance is null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        if (!string.Equals(instance.ActorId, ActorId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Condition '{instance.ConditionId}' belongs to actor '{instance.ActorId}', not '{ActorId}'.");
        }

        _conditions.Remove(instance);
    }

    public IReadOnlyList<ConditionInstance> GetActiveConditions()
    {
        return _conditions
            .Where(static c => !c.IsExpired)
            .ToArray();
    }

    public IReadOnlyList<ConditionInstance> GetActiveConditionsClearedByDamage()
    {
        return _conditions
            .Where(static c => !c.IsExpired && c.ClearedByDamage)
            .ToArray();
    }

    public void ClearConditionsRemovedByDamage()
    {
        _conditions.RemoveAll(static c => !c.IsExpired && c.ClearedByDamage);
    }

    public void SetDefending(bool value)
    {
        IsDefending = value;
    }

    public void ClearDefending()
    {
        IsDefending = false;
    }
}