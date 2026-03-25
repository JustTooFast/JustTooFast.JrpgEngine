// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

public sealed class NeverBlockOnPlayerChoicePolicy : IPlayerChoiceBlockingPolicy
{
    public bool ShouldBlockAdvance(BattleRuntimeView runtimeView)
    {
        if (runtimeView is null)
        {
            throw new ArgumentNullException(nameof(runtimeView));
        }

        return false;
    }
}