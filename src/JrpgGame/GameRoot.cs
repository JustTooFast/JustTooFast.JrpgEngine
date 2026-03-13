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
using DisplayMode = JustTooFast.JrpgEngine.Rendering.DisplayMode;

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
    private PresentationSurface? _presentationSurface;

    private Texture2D? _debugPixel;
    private SpriteFont? _debugFont;
    private SpriteFont? _dialogueFont;

    private IVisualTextureStore? _visualTextureStore;
    private IMapBackgroundRenderer? _mapBackgroundRenderer;
    private MapOverheadRenderer? _mapOverheadRenderer;
    private IPlayerRenderer? _playerRenderer;
    private IMapObjectRenderer? _mapObjectRenderer;
    private PauseMenuOverlay? _pauseMenuOverlay;
    private DialogueOverlay? _dialogueOverlay;

    public GameRoot()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.SynchronizeWithVerticalRetrace = true;

        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.Title = "JustTooFast.JrpgGame";
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _debugPixel = DebugTextureFactory.CreateSolidTexture(GraphicsDevice, Color.White);
        _debugFont = Content.Load<SpriteFont>("Fonts/DebugFont");
        _dialogueFont = Content.Load<SpriteFont>("Fonts/DialogueFont");

        var dataRoot = ResolveDataRoot();

        _definitions = DefinitionLoader.LoadAll(dataRoot);
        _runtimeStateValidator = new RuntimeStateValidator();
        _newGameService = new NewGameService(_runtimeStateValidator);
        _mapCollisionService = new MapCollisionService(_definitions);
        _encounterService = new EncounterService();
        _sceneManager = new SceneManager();

        ApplyDisplayMode(_definitions.GameConfig.DisplayMode);

        _visualTextureStore = new VisualTextureStore(Content);

        var debugMapBackgroundRenderer = new DebugMapBackgroundRenderer(_debugPixel);
        var realMapBackgroundRenderer = new RealMapBackgroundRenderer(_visualTextureStore);

        _mapBackgroundRenderer = new MapBackgroundRenderer(
            _definitions.GameConfig.PresentationMode,
            debugMapBackgroundRenderer,
            realMapBackgroundRenderer);

        _mapOverheadRenderer = new MapOverheadRenderer(_visualTextureStore);

        var debugPlayerRenderer = new DebugPlayerRenderer(_debugPixel);
        var realPlayerRenderer = new RealPlayerRenderer(
            _definitions,
            _visualTextureStore);

        _playerRenderer = new PlayerRenderer(
            _definitions.GameConfig.PresentationMode,
            debugPlayerRenderer,
            realPlayerRenderer);

        var debugMapObjectRenderer = new DebugMapObjectRenderer(_debugPixel, _definitions);
        var realMapObjectRenderer = new RealMapObjectRenderer(_visualTextureStore, _definitions);

        _mapObjectRenderer = new MapObjectRenderer(
            _definitions.GameConfig.PresentationMode,
            debugMapObjectRenderer,
            realMapObjectRenderer);

        _pauseMenuOverlay = new PauseMenuOverlay(_debugPixel);
        _dialogueOverlay = new DialogueOverlay(_debugPixel, _dialogueFont);

        _presentationSurface = new PresentationSurface(GraphicsDevice);

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
        if (_spriteBatch is null)
        {
            base.Draw(gameTime);
            return;
        }

        if (_presentationSurface is null)
        {
            throw new InvalidOperationException("PresentationSurface has not been initialized.");
        }

        _presentationSurface.BeginWorldScene();
        _sceneManager?.DrawWorld(gameTime, _spriteBatch);
        _presentationSurface.EndWorldScene();

        _presentationSurface.BeginUiScene();
        _sceneManager?.DrawUi(gameTime, _spriteBatch);
        _presentationSurface.EndUiScene();

        _presentationSurface.Present(_spriteBatch);

        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _presentationSurface?.Dispose();
        }

        base.Dispose(disposing);
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

        if (_mapBackgroundRenderer is null)
        {
            throw new InvalidOperationException("MapBackgroundRenderer has not been initialized.");
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
            _mapBackgroundRenderer,
            _mapOverheadRenderer,
            _playerRenderer,
            _mapObjectRenderer,
            _pauseMenuOverlay,
            _dialogueOverlay,
            _definitions.GameConfig.PresentationMode,
            CreateMapScene);
    }

    private void ApplyDisplayMode(DisplayMode displayMode)
    {
        switch (displayMode)
        {
            case DisplayMode.Windowed:
                _graphics.IsFullScreen = false;
                _graphics.HardwareModeSwitch = false;
                Window.IsBorderless = false;
                Window.AllowUserResizing = false;
                _graphics.PreferredBackBufferWidth = PresentationSurface.InternalWidth * PresentationSurface.WindowedScale;
                _graphics.PreferredBackBufferHeight = PresentationSurface.InternalHeight * PresentationSurface.WindowedScale;
                _graphics.ApplyChanges();
                break;

            case DisplayMode.Fullscreen:
                Window.AllowUserResizing = false;

                if (OperatingSystem.IsLinux())
                {
                    Window.IsBorderless = false;
                    _graphics.HardwareModeSwitch = false;
                    _graphics.IsFullScreen = true;
                    _graphics.ApplyChanges();
                }
                else
                {
                    _graphics.IsFullScreen = false;
                    _graphics.HardwareModeSwitch = false;
                    Window.IsBorderless = true;

                    var displayModeBounds = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
                    _graphics.PreferredBackBufferWidth = displayModeBounds.Width;
                    _graphics.PreferredBackBufferHeight = displayModeBounds.Height;
                    _graphics.ApplyChanges();

                    Window.Position = Point.Zero;
                }

                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported display mode '{displayMode}'.");
        }
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