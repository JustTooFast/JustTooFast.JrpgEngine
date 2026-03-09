// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JustTooFast.JrpgEngine.Core;

public sealed class SceneManager
{
    private IScene? _currentScene;

    public IScene? CurrentScene => _currentScene;

    public SceneType? CurrentSceneType { get; private set; }

    public void ChangeScene(SceneType sceneType, IScene newScene)
    {
        if (newScene is null)
        {
            throw new ArgumentNullException(nameof(newScene));
        }

        _currentScene?.Exit();

        _currentScene = newScene;
        CurrentSceneType = sceneType;

        _currentScene.Enter();
    }

    public void Update(GameTime gameTime)
    {
        _currentScene?.Update(gameTime);
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        _currentScene?.Draw(gameTime, spriteBatch);
    }
}