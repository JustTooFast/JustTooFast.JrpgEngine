// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using JustTooFast.JrpgEngine.Core;
using JustTooFast.JrpgEngine.Definitions;
using JustTooFast.JrpgEngine.Dialogue;
using JustTooFast.JrpgEngine.Maps;
using JustTooFast.JrpgEngine.Menus;
using JustTooFast.JrpgEngine.Rendering;
using JustTooFast.JrpgEngine.Scenes;
using JustTooFast.JrpgEngine.State;
using JustTooFast.JrpgEngine.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JustTooFast.JrpgGame;

public sealed class GameRoot : Game
{
    private readonly GraphicsDeviceManager _graphics;

    private SpriteBatch? _spriteBatch;
    private SceneManager? _sceneManager;
    private DefinitionDatabase? _definitions;
    private RuntimeStateValidator? _runtimeStateValidator;
    private NewGameService? _newGameService;
    private MapCollisionService? _mapCollisionService;
    private EncounterService? _encounterService;

    private Texture2D? _debugPixel;
    private SpriteFont? _debugFont;
    private DebugMapRenderer? _debugMapRenderer;
    private RealMapRenderer? _realMapRenderer;
    private MapOverheadRenderer? _mapOverheadRenderer;
    private PlayerRenderer? _playerRenderer;
    private MapObjectRenderer? _mapObjectRenderer;
    private MapVisualTextureStore? _mapVisualTextureStore;
    private PauseMenuOverlay? _pauseMenuOverlay;
    private DialogueOverlay? _dialogueOverlay;

    public GameRoot()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.Title = "JustTooFast.JrpgGame";
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _debugPixel = DebugTextureFactory.CreateSolidTexture(GraphicsDevice, Color.White);
        _debugFont = Content.Load<SpriteFont>("Fonts/DebugFont");

        _debugMapRenderer = new DebugMapRenderer(_debugPixel);
        _mapVisualTextureStore = new MapVisualTextureStore(Content);
        _realMapRenderer = new RealMapRenderer(_mapVisualTextureStore);
        _mapOverheadRenderer = new MapOverheadRenderer(_mapVisualTextureStore);
        _playerRenderer = new PlayerRenderer(_debugPixel);
        _mapObjectRenderer = new MapObjectRenderer(_debugPixel);
        _pauseMenuOverlay = new PauseMenuOverlay(_debugPixel);
        _dialogueOverlay = new DialogueOverlay(_debugPixel, _debugFont);

        var dataRoot = ResolveDataRoot();

        _definitions = DefinitionLoader.LoadAll(dataRoot);
        _runtimeStateValidator = new RuntimeStateValidator();
        _newGameService = new NewGameService(_runtimeStateValidator);
        _mapCollisionService = new MapCollisionService();
        _encounterService = new EncounterService();
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
        else if (_sceneManager?.CurrentSceneType == SceneType.Battle)
        {
            clearColor = Color.DarkRed;
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
        if (_sceneManager is null)
        {
            throw new InvalidOperationException("SceneManager has not been initialized.");
        }

        if (_definitions is null)
        {
            throw new InvalidOperationException("Definitions have not been loaded.");
        }

        if (_mapCollisionService is null)
        {
            throw new InvalidOperationException("MapCollisionService has not been initialized.");
        }

        if (_encounterService is null)
        {
            throw new InvalidOperationException("EncounterService has not been initialized.");
        }

        if (_debugMapRenderer is null)
        {
            throw new InvalidOperationException("DebugMapRenderer has not been initialized.");
        }

        if (_realMapRenderer is null)
        {
            throw new InvalidOperationException("RealMapRenderer has not been initialized.");
        }

        if (_mapOverheadRenderer is null)
        {
            throw new InvalidOperationException("MapOverheadRenderer has not been initialized.");
        }

        if (_playerRenderer is null)
        {
            throw new InvalidOperationException("PlayerRenderer has not been initialized.");
        }

        if (_mapObjectRenderer is null)
        {
            throw new InvalidOperationException("MapObjectRenderer has not been initialized.");
        }

        if (_pauseMenuOverlay is null)
        {
            throw new InvalidOperationException("PauseMenuOverlay has not been initialized.");
        }

        if (_dialogueOverlay is null)
        {
            throw new InvalidOperationException("DialogueOverlay has not been initialized.");
        }

        return new MapScene(
            _sceneManager,
            _definitions,
            gameState,
            _mapCollisionService,
            _encounterService,
            _debugMapRenderer,
            _realMapRenderer,
            _mapOverheadRenderer,
            _playerRenderer,
            _mapObjectRenderer,
            _pauseMenuOverlay,
            _dialogueOverlay,
            _definitions.GameConfig.PresentationMode,
            CreateMapScene);
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