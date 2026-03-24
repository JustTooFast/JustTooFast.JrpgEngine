// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

public sealed class FirstLivingBattleFlow : IBattleFlow
{
    public string GetNextActorId(BattleState state)
    {
        if (state is null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        var actor = state.Combatants.FirstOrDefault(c => c.IsAlive);

        if (actor is null)
        {
            throw new InvalidOperationException("No living combatants found.");
        }

        return actor.Id;
    }
}