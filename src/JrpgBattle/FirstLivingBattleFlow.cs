// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

public sealed class FirstLivingBattleFlow : IBattleFlow
{
    public BattleFlowStep Advance(BattleState state)
    {
        if (state is null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        BattleCombatantState? actor = state.Combatants.FirstOrDefault(c => !c.IsDefeated);

        if (actor is null)
        {
            return new BattleFlowStep(
                hasAdvanced: false,
                readyActorId: null);
        }

        return new BattleFlowStep(
            hasAdvanced: true,
            readyActorId: actor.Id);
    }
}