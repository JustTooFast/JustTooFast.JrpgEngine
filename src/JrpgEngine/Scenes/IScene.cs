// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JustTooFast.JrpgEngine.Core;

public interface IScene
{
    void Enter();

    void Exit();

    void Update(GameTime gameTime);

    void DrawWorld(GameTime gameTime, SpriteBatch spriteBatch);

    void DrawUi(GameTime gameTime, SpriteBatch spriteBatch);
}