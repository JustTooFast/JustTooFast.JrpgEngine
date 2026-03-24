// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

public sealed class FirstLivingBattleFlow : IBattleFlow
{
    private string? _readyActorId;

    public BattleFlowResult Advance(BattleState state)
    {
        if (state is null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        if (!string.IsNullOrWhiteSpace(_readyActorId))
        {
            return new BattleFlowResult(
                HasChanged: false,
                ReadyActorId: _readyActorId);
        }

        BattleCombatantState? actor = state.Combatants.FirstOrDefault(c => c.IsAlive);
        if (actor is null)
        {
            return new BattleFlowResult(
                HasChanged: false,
                ReadyActorId: null);
        }

        _readyActorId = actor.Id;

        return new BattleFlowResult(
            HasChanged: true,
            ReadyActorId: _readyActorId);
    }

    public void ConsumeReadyActor(string actorId)
    {
        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException("Actor id is required.", nameof(actorId));
        }

        if (!string.Equals(_readyActorId, actorId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Ready actor does not match the actor being consumed.");
        }

        _readyActorId = null;
    }
}