using System;
using System.Collections.Generic;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle.Tests.Fakes;

internal sealed class ScriptedBattleFlow : IBattleFlow
{
    private readonly Queue<BattleFlowStep> _steps;

    public ScriptedBattleFlow(IEnumerable<BattleFlowStep> steps)
    {
        if (steps is null)
        {
            throw new ArgumentNullException(nameof(steps));
        }

        _steps = new Queue<BattleFlowStep>(steps);
    }

    public BattleFlowStep Advance(BattleFlowState state)
    {
        if (state is null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        if (_steps.Count == 0)
        {
            return new BattleFlowStep(
                hasAdvanced: false,
                readyActorId: null);
        }

        return _steps.Dequeue();
    }
}