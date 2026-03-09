// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.Core;
using JustTooFast.JrpgEngine.Definitions;
using JustTooFast.JrpgEngine.State;
using JustTooFast.JrpgEngine.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JustTooFast.JrpgEngine.Scenes;

public sealed class TitleScene : IScene
{
    private readonly SceneManager _sceneManager;
    private readonly DefinitionDatabase _definitions;
    private readonly NewGameService _newGameService;
    private readonly Func<GameState, MapScene> _mapSceneFactory;

    private KeyboardState _previousKeyboardState;

    public TitleScene(
        SceneManager sceneManager,
        DefinitionDatabase definitions,
        NewGameService newGameService,
        Func<GameState, MapScene> mapSceneFactory)
    {
        _sceneManager = sceneManager ?? throw new ArgumentNullException(nameof(sceneManager));
        _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
        _newGameService = newGameService ?? throw new ArgumentNullException(nameof(newGameService));
        _mapSceneFactory = mapSceneFactory ?? throw new ArgumentNullException(nameof(mapSceneFactory));
    }

    public void Enter()
    {
        _previousKeyboardState = Keyboard.GetState();
    }

    public void Exit()
    {
    }

    public void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();

        if (WasKeyJustPressed(Keys.Enter, keyboardState))
        {
            StartNewGame();
        }

        _previousKeyboardState = keyboardState;
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        // Keep this empty for the moment.
        // You can still tell the scene is active by using different clear colors in GameRoot.
    }

    private void StartNewGame()
    {
        var gameState = _newGameService.CreateNewGame(_definitions);
        var mapScene = _mapSceneFactory(gameState);

        _sceneManager.ChangeScene(SceneType.Map, mapScene);
    }

    private bool WasKeyJustPressed(Keys key, KeyboardState currentKeyboardState)
    {
        return currentKeyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
    }
}