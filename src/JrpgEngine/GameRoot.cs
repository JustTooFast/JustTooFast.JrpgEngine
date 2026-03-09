// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using JustTooFast.JrpgEngine.Core;
using JustTooFast.JrpgEngine.Definitions;
using JustTooFast.JrpgEngine.Maps;
using JustTooFast.JrpgEngine.Menus;
using JustTooFast.JrpgEngine.Rendering;
using JustTooFast.JrpgEngine.Scenes;
using JustTooFast.JrpgEngine.State;
using JustTooFast.JrpgEngine.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JustTooFast.JrpgEngine;

public sealed class GameRoot : Game
{
    private readonly GraphicsDeviceManager _graphics;

    private SpriteBatch? _spriteBatch;
    private SceneManager? _sceneManager;
    private DefinitionDatabase? _definitions;
    private RuntimeStateValidator? _runtimeStateValidator;
    private NewGameService? _newGameService;
    private MapCollisionService? _mapCollisionService;

    private Texture2D? _debugPixel;
    private MapRenderer? _mapRenderer;
    private PlayerRenderer? _playerRenderer;
    private PauseMenuOverlay? _pauseMenuOverlay;

    public GameRoot()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.Title = "JustTooFast.JrpgEngine";
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _debugPixel = DebugTextureFactory.CreateSolidTexture(GraphicsDevice, Color.White);
        _mapRenderer = new MapRenderer(_debugPixel);
        _playerRenderer = new PlayerRenderer(_debugPixel);
        _pauseMenuOverlay = new PauseMenuOverlay(_debugPixel);

        var dataRoot = ResolveDataRoot();

        _definitions = DefinitionLoader.LoadAll(dataRoot);
        _runtimeStateValidator = new RuntimeStateValidator();
        _newGameService = new NewGameService(_runtimeStateValidator);
        _mapCollisionService = new MapCollisionService();
        _sceneManager = new SceneManager();

        var titleScene = new TitleScene(
            _sceneManager,
            _definitions,
            _newGameService,
            CreateMapScene);

        _sceneManager.ChangeScene(SceneType.Title, titleScene);
    }

    protected override void Update(GameTime gameTime)
    {
        if (_sceneManager?.CurrentSceneType == SceneType.Title &&
            Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Exit();
            return;
        }

        _sceneManager?.Update(gameTime);

        if (_sceneManager?.CurrentScene is MapScene mapScene && mapScene.RequestReturnToTitle)
        {
            if (_definitions is null || _newGameService is null || _sceneManager is null)
            {
                throw new InvalidOperationException("GameRoot is missing required services.");
            }

            var titleScene = new TitleScene(
                _sceneManager,
                _definitions,
                _newGameService,
                CreateMapScene);

            _sceneManager.ChangeScene(SceneType.Title, titleScene);
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        var clearColor = Color.CornflowerBlue;

        if (_sceneManager?.CurrentSceneType == SceneType.Map)
        {
            clearColor = Color.DarkOliveGreen;
        }

        GraphicsDevice.Clear(clearColor);

        if (_spriteBatch is not null)
        {
            _sceneManager?.Draw(gameTime, _spriteBatch);
        }

        base.Draw(gameTime);
    }

    private MapScene CreateMapScene(GameState gameState)
    {
        if (_definitions is null)
        {
            throw new InvalidOperationException("Definitions have not been loaded.");
        }

        if (_mapCollisionService is null)
        {
            throw new InvalidOperationException("MapCollisionService has not been initialized.");
        }

        if (_mapRenderer is null)
        {
            throw new InvalidOperationException("MapRenderer has not been initialized.");
        }

        if (_playerRenderer is null)
        {
            throw new InvalidOperationException("PlayerRenderer has not been initialized.");
        }

        if (_pauseMenuOverlay is null)
        {
            throw new InvalidOperationException("PauseMenuOverlay has not been initialized.");
        }

        return new MapScene(
            _definitions,
            gameState,
            _mapCollisionService,
            _mapRenderer,
            _playerRenderer,
            _pauseMenuOverlay);
    }

    private static string ResolveDataRoot()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var candidate = Path.GetFullPath(
            Path.Combine(baseDirectory, "..", "..", "..", "..", "..", "data"));

        if (!Directory.Exists(candidate))
        {
            throw new DirectoryNotFoundException(
                $"Could not find data directory. Checked: {candidate}");
        }

        return candidate;
    }
}