// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.Definitions;
using JustTooFast.JrpgEngine.Maps;
using JustTooFast.JrpgEngine.Menus;
using JustTooFast.JrpgEngine.Rendering;
using JustTooFast.JrpgEngine.State;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JustTooFast.JrpgEngine.Scenes;

public sealed class MapScene : IScene
{
    private readonly DefinitionDatabase _definitions;
    private readonly MapCollisionService _mapCollisionService;
    private readonly PlayerMapMover _playerMapMover;
    private readonly MapRenderer _mapRenderer;
    private readonly PlayerRenderer _playerRenderer;
    private readonly PauseMenuOverlay _pauseMenuOverlay;

    private KeyboardState _previousKeyboardState;
    private FacingDirection? _heldMoveDirection;
    private double _nextHeldMoveAllowedTimeMs;

    private const double HeldMoveRepeatDelayMs = 140.0;
    private const double HeldBlockedRepeatDelayMs = 180.0;

    public MapScene(
        DefinitionDatabase definitions,
        GameState gameState,
        MapCollisionService mapCollisionService,
        MapRenderer mapRenderer,
        PlayerRenderer playerRenderer,
        PauseMenuOverlay pauseMenuOverlay)
    {
        _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
        GameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
        _mapCollisionService = mapCollisionService ?? throw new ArgumentNullException(nameof(mapCollisionService));
        _mapRenderer = mapRenderer ?? throw new ArgumentNullException(nameof(mapRenderer));
        _playerRenderer = playerRenderer ?? throw new ArgumentNullException(nameof(playerRenderer));
        _pauseMenuOverlay = pauseMenuOverlay ?? throw new ArgumentNullException(nameof(pauseMenuOverlay));

        RuntimeMap = new MapRuntime(GetCurrentMapDef());
        _playerMapMover = new PlayerMapMover(_mapCollisionService);
        _playerMapMover.InitializeFromGameState(GameState);
    }

    public GameState GameState { get; }

    public MapRuntime RuntimeMap { get; }

    public bool RequestReturnToTitle { get; private set; }

    public TileCoord GetPlayerTile()
    {
        return new TileCoord(GameState.PlayerTileX, GameState.PlayerTileY);
    }

    public bool CanPlayerEnterTile(TileCoord tile)
    {
        return _mapCollisionService.CanEnterTile(RuntimeMap.Definition, tile);
    }

    public void Enter()
    {
        _previousKeyboardState = Keyboard.GetState();
        _heldMoveDirection = null;
        _nextHeldMoveAllowedTimeMs = 0.0;
        RequestReturnToTitle = false;
        _pauseMenuOverlay.Reset();
        GameState.IsPaused = false;
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

        var pauseResult = _pauseMenuOverlay.Update(gameTime);
        GameState.IsPaused = _pauseMenuOverlay.IsOpen;

        if (pauseResult == PauseMenuResult.ReturnToTitle)
        {
            RequestReturnToTitle = true;
            return;
        }

        if (_pauseMenuOverlay.IsOpen)
        {
            _heldMoveDirection = null;
            _nextHeldMoveAllowedTimeMs = 0.0;
            _previousKeyboardState = Keyboard.GetState();
            return;
        }

        var keyboardState = Keyboard.GetState();

        if (!_playerMapMover.IsMoving)
        {
            HandleMovementInput(gameTime, keyboardState);
        }

        _playerMapMover.Update(gameTime, GameState, RuntimeMap.Definition);

        _previousKeyboardState = keyboardState;
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        _mapRenderer.Draw(spriteBatch, RuntimeMap);

        var playerWorldPosition = _playerMapMover.GetVisualWorldPosition(GameState, RuntimeMap.TileSize);
        _playerRenderer.Draw(spriteBatch, playerWorldPosition, RuntimeMap.TileSize);

        if (_pauseMenuOverlay.IsOpen)
        {
            _pauseMenuOverlay.Draw(spriteBatch, spriteBatch.GraphicsDevice.Viewport);
        }
    }

    private void HandleMovementInput(GameTime gameTime, KeyboardState keyboardState)
    {
        var resolvedDirection = ResolveHeldDirection(keyboardState);

        if (resolvedDirection is null)
        {
            _heldMoveDirection = null;
            _nextHeldMoveAllowedTimeMs = 0.0;
            return;
        }

        var direction = resolvedDirection.Value;
        var isNewPress = IsDirectionNewPress(direction, keyboardState);
        var nowMs = gameTime.TotalGameTime.TotalMilliseconds;

        if (isNewPress || _heldMoveDirection != direction)
        {
            TryMoveWithRepeatScheduling(direction, nowMs);
            _heldMoveDirection = direction;
            return;
        }

        if (nowMs >= _nextHeldMoveAllowedTimeMs)
        {
            TryMoveWithRepeatScheduling(direction, nowMs);
            _heldMoveDirection = direction;
        }
    }

    private void TryMoveWithRepeatScheduling(FacingDirection direction, double nowMs)
    {
        var currentTile = GetPlayerTile();
        var destinationTile = currentTile + GetDirectionOffset(direction);

        _playerMapMover.TryBeginMove(direction, GameState, RuntimeMap.Definition);

        if (_playerMapMover.IsMoving)
        {
            _nextHeldMoveAllowedTimeMs = nowMs + HeldMoveRepeatDelayMs;
            return;
        }

        if (!_mapCollisionService.CanEnterTile(RuntimeMap.Definition, destinationTile))
        {
            _nextHeldMoveAllowedTimeMs = nowMs + HeldBlockedRepeatDelayMs;
            return;
        }

        _nextHeldMoveAllowedTimeMs = nowMs + HeldMoveRepeatDelayMs;
    }

    private FacingDirection? ResolveHeldDirection(KeyboardState keyboardState)
    {
        if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W))
        {
            return FacingDirection.Up;
        }

        if (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S))
        {
            return FacingDirection.Down;
        }

        if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
        {
            return FacingDirection.Left;
        }

        if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
        {
            return FacingDirection.Right;
        }

        return null;
    }

    private bool IsDirectionNewPress(FacingDirection direction, KeyboardState currentKeyboardState)
    {
        return direction switch
        {
            FacingDirection.Up => IsAnyKeyJustPressed(currentKeyboardState, Keys.Up, Keys.W),
            FacingDirection.Down => IsAnyKeyJustPressed(currentKeyboardState, Keys.Down, Keys.S),
            FacingDirection.Left => IsAnyKeyJustPressed(currentKeyboardState, Keys.Left, Keys.A),
            FacingDirection.Right => IsAnyKeyJustPressed(currentKeyboardState, Keys.Right, Keys.D),
            _ => false
        };
    }

    private bool IsAnyKeyJustPressed(KeyboardState currentKeyboardState, Keys primary, Keys alternate)
    {
        var primaryPressed =
            currentKeyboardState.IsKeyDown(primary) &&
            !_previousKeyboardState.IsKeyDown(primary);

        var alternatePressed =
            currentKeyboardState.IsKeyDown(alternate) &&
            !_previousKeyboardState.IsKeyDown(alternate);

        return primaryPressed || alternatePressed;
    }

    private static TileCoord GetDirectionOffset(FacingDirection direction)
    {
        return direction switch
        {
            FacingDirection.Up => new TileCoord(0, -1),
            FacingDirection.Down => new TileCoord(0, 1),
            FacingDirection.Left => new TileCoord(-1, 0),
            FacingDirection.Right => new TileCoord(1, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Unsupported direction.")
        };
    }

    private MapDef GetCurrentMapDef()
    {
        if (!_definitions.Maps.TryGetValue(GameState.CurrentMapId, out var mapDef))
        {
            throw new InvalidOperationException(
                $"MapScene could not find current map '{GameState.CurrentMapId}'.");
        }

        return mapDef;
    }
}