// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JustTooFast.JrpgEngine.Core;

public sealed class SceneManager
{
    public IScene? CurrentScene { get; private set; }

    public SceneType CurrentSceneType { get; private set; }

    public void ChangeScene(SceneType sceneType, IScene newScene)
    {
        if (newScene is null)
        {
            throw new ArgumentNullException(nameof(newScene));
        }

        CurrentScene?.Exit();

        CurrentScene = newScene;
        CurrentSceneType = sceneType;

        CurrentScene.Enter();
    }

    public void Update(GameTime gameTime)
    {
        CurrentScene?.Update(gameTime);
    }

    public void DrawWorld(GameTime gameTime, SpriteBatch spriteBatch)
    {
        CurrentScene?.DrawWorld(gameTime, spriteBatch);
    }

    public void DrawUi(GameTime gameTime, SpriteBatch spriteBatch)
    {
        CurrentScene?.DrawUi(gameTime, spriteBatch);
    }
}