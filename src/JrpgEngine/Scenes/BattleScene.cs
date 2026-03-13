// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using JustTooFast.JrpgEngine.Core;
using JustTooFast.JrpgEngine.Definitions;
using JustTooFast.JrpgEngine.Rendering;
using JustTooFast.JrpgEngine.State;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JustTooFast.JrpgEngine.Scenes;

public sealed class BattleScene : IScene
{
    private readonly SceneManager _sceneManager;
    private readonly DefinitionDatabase _definitions;
    private readonly GameState _gameState;
    private readonly EncounterDef _encounter;
    private readonly Func<GameState, MapScene> _mapSceneFactory;

    private readonly List<EnemyDef> _enemies = new();

    private KeyboardState _previousKeyboardState;
    private Texture2D? _backgroundPixel;

    public BattleScene(
        SceneManager sceneManager,
        DefinitionDatabase definitions,
        GameState gameState,
        EncounterDef encounter,
        Func<GameState, MapScene> mapSceneFactory)
    {
        _sceneManager = sceneManager ?? throw new ArgumentNullException(nameof(sceneManager));
        _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
        _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
        _encounter = encounter ?? throw new ArgumentNullException(nameof(encounter));
        _mapSceneFactory = mapSceneFactory ?? throw new ArgumentNullException(nameof(mapSceneFactory));

        ResolveEncounterEnemies();
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

        if (WasConfirmJustPressed(keyboardState))
        {
            ReturnToMap();
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
            Color.DarkRed);

        spriteBatch.End();
    }

    public void DrawUi(GameTime gameTime, SpriteBatch spriteBatch)
    {
    }

    private void ResolveEncounterEnemies()
    {
        foreach (var enemyId in _encounter.EnemyIds)
        {
            if (!_definitions.Enemies.TryGetValue(enemyId, out var enemyDef))
            {
                throw new InvalidOperationException(
                    $"BattleScene could not resolve enemy '{enemyId}' from encounter '{_encounter.Id}'.");
            }

            _enemies.Add(enemyDef);
        }
    }

    private void ReturnToMap()
    {
        var mapScene = _mapSceneFactory(_gameState);
        _sceneManager.ChangeScene(SceneType.Map, mapScene);
    }

    private bool WasConfirmJustPressed(KeyboardState currentKeyboardState)
    {
        return IsAnyKeyJustPressed(currentKeyboardState, Keys.Space, Keys.Enter);
    }

    private bool IsAnyKeyJustPressed(
        KeyboardState currentKeyboardState,
        Keys primary,
        Keys alternate)
    {
        var primaryPressed =
            currentKeyboardState.IsKeyDown(primary) &&
            !_previousKeyboardState.IsKeyDown(primary);

        var alternatePressed =
            currentKeyboardState.IsKeyDown(alternate) &&
            !_previousKeyboardState.IsKeyDown(alternate);

        return primaryPressed || alternatePressed;
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