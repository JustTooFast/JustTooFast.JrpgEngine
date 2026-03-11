// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;

namespace JustTooFast.JrpgGame;

public static class Program
{
    [STAThread]
    private static void Main()
    {
        using var game = new GameRoot();
        game.Run();
    }
}