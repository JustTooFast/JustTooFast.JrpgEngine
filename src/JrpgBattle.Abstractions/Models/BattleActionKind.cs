// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public enum BattleActionKind
{
    Attack = 0,
    Defend = 1,
    Escape = 2,
    Magic = 3,
    Item = 4,
    Skill = 5,
    Wait = 6,
}