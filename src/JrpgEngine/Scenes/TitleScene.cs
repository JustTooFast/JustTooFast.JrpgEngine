// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.Core;
using JustTooFast.JrpgEngine.Definitions;
using JustTooFast.JrpgEngine.Rendering;
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
    private Texture2D? _backgroundPixel;

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
        if (gameTime is null)
        {
            throw new ArgumentNullException(nameof(gameTime));
        }

        var keyboardState = Keyboard.GetState();

        if (WasKeyJustPressed(Keys.Enter, keyboardState))
        {
            StartNewGame();
        }

        _previousKeyboardState = keyboardState;
    }

    public void DrawWorld(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (gameTime is null)
        {
            throw new ArgumentNullException(nameof(gameTime));
        }

        if (spriteBatch is null)
        {
            throw new ArgumentNullException(nameof(spriteBatch));
        }

        EnsureBackgroundPixel(spriteBatch.GraphicsDevice);

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullNone);

        spriteBatch.Draw(
            _backgroundPixel!,
            new Rectangle(0, 0, PresentationSurface.InternalWidth, PresentationSurface.InternalHeight),
            Color.CornflowerBlue);

        spriteBatch.End();
    }

    public void DrawUi(GameTime gameTime, SpriteBatch spriteBatch)
    {
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

    private void EnsureBackgroundPixel(GraphicsDevice graphicsDevice)
    {
        if (graphicsDevice is null)
        {
            throw new ArgumentNullException(nameof(graphicsDevice));
        }

        if (_backgroundPixel is not null && !_backgroundPixel.IsDisposed)
        {
            return;
        }

        _backgroundPixel = new Texture2D(graphicsDevice, 1, 1);
        _backgroundPixel.SetData(new[] { Color.White });
    }
}