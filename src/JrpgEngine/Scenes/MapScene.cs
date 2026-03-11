// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using JustTooFast.JrpgEngine.Core;
using JustTooFast.JrpgEngine.Definitions;
using JustTooFast.JrpgEngine.Dialogue;
using JustTooFast.JrpgEngine.Interactions;
using JustTooFast.JrpgEngine.Maps;
using JustTooFast.JrpgEngine.Menus;
using JustTooFast.JrpgEngine.Rendering;
using JustTooFast.JrpgEngine.State;
using JustTooFast.JrpgEngine.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JustTooFast.JrpgEngine.Scenes;

public sealed class MapScene : IScene
{
    private readonly DefinitionDatabase _definitions;
    private readonly MapCollisionService _mapCollisionService;
    private readonly PlayerMapMover _playerMapMover;
    private readonly DebugMapRenderer _debugMapRenderer;
    private readonly RealMapRenderer _realMapRenderer;
    private readonly PlayerRenderer _playerRenderer;
    private readonly MapObjectRenderer _mapObjectRenderer;
    private readonly PresentationMode _presentationMode;
    private readonly PauseMenuOverlay _pauseMenuOverlay;
    private readonly DialogueOverlay _dialogueOverlay;
    private readonly MapInteractionRunner _interactionRunner;
    private readonly MapStateResolver _mapStateResolver;
    private readonly EncounterService _encounterService;
    private readonly SceneManager _sceneManager;
    private readonly Func<GameState, MapScene> _mapSceneFactory;

    private KeyboardState _previousKeyboardState;
    private FacingDirection? _heldMoveDirection;
    private double _nextHeldMoveAllowedTimeMs;
    private DialogueSession? _activeDialogue;
    private MapControlState _controlState;

    private const double HeldMoveRepeatDelayMs = 140.0;
    private const double HeldBlockedRepeatDelayMs = 180.0;

    public MapScene(
        SceneManager sceneManager,
        DefinitionDatabase definitions,
        GameState gameState,
        MapCollisionService mapCollisionService,
        EncounterService encounterService,
        DebugMapRenderer debugMapRenderer,
        RealMapRenderer realMapRenderer,
        PlayerRenderer playerRenderer,
        MapObjectRenderer mapObjectRenderer,
        PauseMenuOverlay pauseMenuOverlay,
        DialogueOverlay dialogueOverlay,
        PresentationMode presentationMode,
        Func<GameState, MapScene> mapSceneFactory)
    {
        _sceneManager = sceneManager ?? throw new ArgumentNullException(nameof(sceneManager));
        _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
        GameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
        _mapCollisionService = mapCollisionService ?? throw new ArgumentNullException(nameof(mapCollisionService));
        _encounterService = encounterService ?? throw new ArgumentNullException(nameof(encounterService));
        _debugMapRenderer = debugMapRenderer ?? throw new ArgumentNullException(nameof(debugMapRenderer));
        _realMapRenderer = realMapRenderer ?? throw new ArgumentNullException(nameof(realMapRenderer));
        _playerRenderer = playerRenderer ?? throw new ArgumentNullException(nameof(playerRenderer));
        _mapObjectRenderer = mapObjectRenderer ?? throw new ArgumentNullException(nameof(mapObjectRenderer));
        _pauseMenuOverlay = pauseMenuOverlay ?? throw new ArgumentNullException(nameof(pauseMenuOverlay));
        _dialogueOverlay = dialogueOverlay ?? throw new ArgumentNullException(nameof(dialogueOverlay));
        _presentationMode = presentationMode;
        _mapSceneFactory = mapSceneFactory ?? throw new ArgumentNullException(nameof(mapSceneFactory));

        _mapStateResolver = new MapStateResolver();
        RuntimeMap = BuildRuntimeMap(GetCurrentMapDef());

        _playerMapMover = new PlayerMapMover();
        _interactionRunner = new MapInteractionRunner(_definitions, GameState);

        _playerMapMover.InitializeFromGameState(GameState);
        _controlState = MapControlState.Normal;
    }

    public GameState GameState { get; }

    public MapRuntime RuntimeMap { get; private set; }

    public bool RequestReturnToTitle { get; private set; }

    public TileCoord GetPlayerTile()
    {
        return new TileCoord(GameState.PlayerTileX, GameState.PlayerTileY);
    }

    public bool CanPlayerEnterTile(TileCoord tile)
    {
        return CanPlayerEnterTileInternal(tile);
    }

    public void Enter()
    {
        _previousKeyboardState = Keyboard.GetState();
        _heldMoveDirection = null;
        _nextHeldMoveAllowedTimeMs = 0.0;
        _activeDialogue = null;
        _controlState = GameState.PendingMapTransition is null
            ? MapControlState.Normal
            : MapControlState.Transitioning;
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

        var keyboardState = Keyboard.GetState();

        if (_controlState == MapControlState.Transitioning)
        {
            UpdateTransition();
            _previousKeyboardState = keyboardState;
            return;
        }

        if (_activeDialogue is not null)
        {
            UpdateDialogue(keyboardState);
            return;
        }

        var pauseResult = _pauseMenuOverlay.Update(gameTime);
        GameState.IsPaused = _pauseMenuOverlay.IsOpen;

        if (pauseResult == PauseMenuResult.ReturnToTitle)
        {
            RequestReturnToTitle = true;
            _previousKeyboardState = keyboardState;
            return;
        }

        if (pauseResult == PauseMenuResult.Resumed)
        {
            _heldMoveDirection = null;
            _nextHeldMoveAllowedTimeMs = 0.0;
            _previousKeyboardState = keyboardState;
            return;
        }

        if (_pauseMenuOverlay.IsOpen)
        {
            _heldMoveDirection = null;
            _nextHeldMoveAllowedTimeMs = 0.0;
            _previousKeyboardState = keyboardState;
            return;
        }

        if (!_playerMapMover.IsMoving && WasInteractJustPressed(keyboardState))
        {
            TryStartFrontTileInteraction();
        }

        var wasMoving = _playerMapMover.IsMoving;

        if (_activeDialogue is null)
        {
            if (!_playerMapMover.IsMoving)
            {
                HandleMovementInput(gameTime, keyboardState);
            }

            _playerMapMover.Update(gameTime, GameState, RuntimeMap.Definition);
        }

        if (wasMoving && !_playerMapMover.IsMoving && _controlState == MapControlState.Normal)
        {
            TryStartStepOnInteraction();

            if (_controlState == MapControlState.Normal)
            {
                TryStartRandomEncounter();
            }
        }

        _previousKeyboardState = keyboardState;
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (gameTime is null)
        {
            throw new ArgumentNullException(nameof(gameTime));
        }

        if (spriteBatch is null)
        {
            throw new ArgumentNullException(nameof(spriteBatch));
        }

        var context = new MapSceneRenderContext(
            spriteBatch,
            gameTime,
            GameState,
            RuntimeMap);

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullNone);

        GetMapBackgroundRenderer().Draw(context);
        _mapObjectRenderer.Draw(spriteBatch, RuntimeMap);

        var playerWorldPosition = _playerMapMover.GetVisualWorldPosition(GameState, RuntimeMap.TileSize);
        _playerRenderer.Draw(spriteBatch, playerWorldPosition, RuntimeMap.TileSize);

        spriteBatch.End();

        if (_activeDialogue is not null)
        {
            _dialogueOverlay.Draw(spriteBatch, spriteBatch.GraphicsDevice.Viewport, _activeDialogue);
            return;
        }

        if (_pauseMenuOverlay.IsOpen)
        {
            _pauseMenuOverlay.Draw(spriteBatch, spriteBatch.GraphicsDevice.Viewport);
        }
    }

    private void UpdateDialogue(KeyboardState keyboardState)
    {
        if (_activeDialogue is null)
        {
            throw new InvalidOperationException("Dialogue update requested with no active dialogue.");
        }

        _heldMoveDirection = null;
        _nextHeldMoveAllowedTimeMs = 0.0;
        GameState.IsPaused = false;

        if (WasConfirmJustPressed(keyboardState))
        {
            var finished = _activeDialogue.Advance();
            if (finished)
            {
                ApplyDialogueResults(_activeDialogue);
                _activeDialogue = null;
                _controlState = MapControlState.Normal;
            }
        }

        _previousKeyboardState = keyboardState;
    }

    private void ApplyDialogueResults(DialogueSession dialogueSession)
    {
        if (dialogueSession is null)
        {
            throw new ArgumentNullException(nameof(dialogueSession));
        }

        var didChangeAnyFlag = false;

        foreach (var result in dialogueSession.Results)
        {
            if (string.Equals(result.Type, "SetFlag", StringComparison.Ordinal))
            {
                if (ApplySetFlagResult(result))
                {
                    didChangeAnyFlag = true;
                }

                continue;
            }

            if (string.Equals(result.Type, "GiveItem", StringComparison.Ordinal))
            {
                if (string.IsNullOrWhiteSpace(result.ItemId))
                {
                    throw new InvalidOperationException("GiveItem result requires a non-empty ItemId.");
                }

                if (result.Amount <= 0)
                {
                    throw new InvalidOperationException("GiveItem result requires Amount > 0.");
                }

                if (!_definitions.Items.ContainsKey(result.ItemId))
                {
                    throw new InvalidOperationException(
                        $"GiveItem result references unknown item '{result.ItemId}'.");
                }

                GameState.Inventory.AddItem(result.ItemId, result.Amount);
                continue;
            }

            throw new InvalidOperationException(
                $"Unsupported dialogue result type '{result.Type}'.");
        }

        if (didChangeAnyFlag)
        {
            RefreshCurrentMapState();
        }
    }

    private bool ApplySetFlagResult(InteractionResultDef result)
    {
        if (string.IsNullOrWhiteSpace(result.FlagId))
        {
            throw new InvalidOperationException("SetFlag result requires a non-empty FlagId.");
        }

        var wasAlreadySet = GameState.StoryFlags.IsSet(result.FlagId);
        GameState.StoryFlags.Set(result.FlagId);
        return !wasAlreadySet;
    }

    private void TryStartFrontTileInteraction()
    {
        var frontObject = TryGetFrontObject();
        if (frontObject is not null && TryStartInteraction(frontObject))
        {
            return;
        }

        var currentObject = TryGetObjectAtTile(GetPlayerTile());
        if (currentObject is null)
        {
            return;
        }

        if (!_definitions.Interactions.TryGetValue(currentObject.InteractionId, out var interaction))
        {
            throw new InvalidOperationException(
                $"Interaction '{currentObject.InteractionId}' was not found.");
        }

        if (!string.Equals(interaction.Type, "MapExit", StringComparison.Ordinal) ||
            interaction.TriggerOnStep)
        {
            return;
        }

        TryStartInteraction(currentObject);
    }

    private void TryStartStepOnInteraction()
    {
        var currentObject = TryGetObjectAtTile(GetPlayerTile());
        if (currentObject is null)
        {
            return;
        }

        if (!_definitions.Interactions.TryGetValue(currentObject.InteractionId, out var interaction))
        {
            throw new InvalidOperationException(
                $"Interaction '{currentObject.InteractionId}' was not found.");
        }

        if (!string.Equals(interaction.Type, "MapExit", StringComparison.Ordinal) ||
            !interaction.TriggerOnStep)
        {
            return;
        }

        TryStartInteraction(currentObject);
    }

    private bool TryStartInteraction(MapObjectDef mapObject)
    {
        var result = _interactionRunner.TryStart(mapObject.InteractionId);
        if (!result.Started)
        {
            return false;
        }

        if (result.DialogueSession is not null)
        {
            _activeDialogue = result.DialogueSession;
            _heldMoveDirection = null;
            _nextHeldMoveAllowedTimeMs = 0.0;
            _controlState = MapControlState.Dialogue;
            return true;
        }

        if (result.PendingMapTransition is not null)
        {
            BeginMapTransition(
                result.PendingMapTransition.DestinationMapId,
                result.PendingMapTransition.DestinationSpawnId);
            return true;
        }

        return false;
    }

    private void TryStartRandomEncounter()
    {
        var encounter = _encounterService.TryTriggerEncounter(
            GameState,
            RuntimeMap.Definition,
            _definitions);

        if (encounter is null)
        {
            return;
        }

        StartBattle(encounter);
    }

    private void StartBattle(EncounterDef encounter)
    {
        if (encounter is null)
        {
            throw new ArgumentNullException(nameof(encounter));
        }

        _heldMoveDirection = null;
        _nextHeldMoveAllowedTimeMs = 0.0;
        _pauseMenuOverlay.Reset();
        GameState.IsPaused = false;
        _activeDialogue = null;

        var battleScene = new BattleScene(
            _sceneManager,
            _definitions,
            GameState,
            encounter,
            _mapSceneFactory);

        _sceneManager.ChangeScene(SceneType.Battle, battleScene);
    }

    private void BeginMapTransition(string destinationMapId, string destinationSpawnId)
    {
        GameState.PendingMapTransition = new PendingMapTransitionState
        {
            DestinationMapId = destinationMapId,
            DestinationSpawnId = destinationSpawnId
        };

        _heldMoveDirection = null;
        _nextHeldMoveAllowedTimeMs = 0.0;
        _pauseMenuOverlay.Reset();
        GameState.IsPaused = false;
        _controlState = MapControlState.Transitioning;
    }

    private void UpdateTransition()
    {
        var pending = GameState.PendingMapTransition;
        if (pending is null)
        {
            _controlState = MapControlState.Normal;
            return;
        }

        if (!_definitions.Maps.TryGetValue(pending.DestinationMapId, out var destinationMap))
        {
            throw new InvalidOperationException(
                $"Pending transition destination map '{pending.DestinationMapId}' was not found.");
        }

        MapSpawnDef? destinationSpawn = null;
        foreach (var spawn in destinationMap.Spawns)
        {
            if (string.Equals(spawn.Id, pending.DestinationSpawnId, StringComparison.Ordinal))
            {
                destinationSpawn = spawn;
                break;
            }
        }

        if (destinationSpawn is null)
        {
            throw new InvalidOperationException(
                $"Pending transition destination spawn '{pending.DestinationSpawnId}' was not found on map '{destinationMap.Id}'.");
        }

        GameState.CurrentMapId = destinationMap.Id;
        GameState.PlayerTileX = destinationSpawn.X;
        GameState.PlayerTileY = destinationSpawn.Y;
        GameState.Facing = destinationSpawn.Facing;

        RuntimeMap = BuildRuntimeMap(destinationMap);
        _playerMapMover.InitializeFromGameState(GameState);

        GameState.PendingMapTransition = null;
        _controlState = MapControlState.Normal;
    }

    private MapObjectDef? TryGetFrontObject()
    {
        var frontTile = GetPlayerTile() + GetDirectionOffset(GameState.Facing);
        return TryGetObjectAtTile(frontTile);
    }

    private MapObjectDef? TryGetObjectAtTile(TileCoord tile)
    {
        foreach (var mapObject in RuntimeMap.Definition.Objects)
        {
            if (mapObject.X == tile.X && mapObject.Y == tile.Y)
            {
                return mapObject;
            }
        }

        return null;
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

        GameState.Facing = direction;

        if (!CanPlayerEnterTileInternal(destinationTile))
        {
            _nextHeldMoveAllowedTimeMs = nowMs + HeldBlockedRepeatDelayMs;
            return;
        }

        _playerMapMover.TryBeginMove(direction, GameState);

        if (_playerMapMover.IsMoving)
        {
            _nextHeldMoveAllowedTimeMs = nowMs + HeldMoveRepeatDelayMs;
            return;
        }

        _nextHeldMoveAllowedTimeMs = nowMs + HeldBlockedRepeatDelayMs;
    }

    private bool CanPlayerEnterTileInternal(TileCoord tile)
    {
        if (!_mapCollisionService.IsInBounds(RuntimeMap.Definition, tile))
        {
            return false;
        }

        foreach (var blockedTile in RuntimeMap.Definition.BlockedTiles)
        {
            if (blockedTile.X == tile.X && blockedTile.Y == tile.Y)
            {
                return false;
            }
        }

        var mapObject = TryGetObjectAtTile(tile);
        if (mapObject is null || !mapObject.BlocksMovement)
        {
            return true;
        }

        if (!_definitions.Interactions.TryGetValue(mapObject.InteractionId, out var interaction))
        {
            throw new InvalidOperationException(
                $"Interaction '{mapObject.InteractionId}' was not found.");
        }

        if (string.Equals(interaction.Type, "LockedDoor", StringComparison.Ordinal))
        {
            return GameState.StoryFlags.IsSet(interaction.OpenFlagId);
        }

        if (string.Equals(interaction.Type, "FlagGate", StringComparison.Ordinal))
        {
            return GameState.StoryFlags.IsSet(interaction.RequiredFlagId);
        }

        return false;
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

    private bool WasInteractJustPressed(KeyboardState currentKeyboardState)
    {
        return IsAnyKeyJustPressed(currentKeyboardState, Keys.Space, Keys.Enter);
    }

    private bool WasConfirmJustPressed(KeyboardState currentKeyboardState)
    {
        return IsAnyKeyJustPressed(currentKeyboardState, Keys.Space, Keys.Enter);
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

    // Rebuild the current runtime map deterministically from source definitions and story flags.
    private MapRuntime BuildRuntimeMap(MapDef sourceMap)
    {
        var resolvedMapState = _mapStateResolver.Resolve(sourceMap, GameState.StoryFlags);

        return new MapRuntime(
            resolvedMapState.EffectiveMapDef,
            resolvedMapState.ActiveVariantId,
            resolvedMapState.VisualAssetId);
    }

    private void RefreshCurrentMapState()
    {
        RuntimeMap = BuildRuntimeMap(GetCurrentMapDef());
    }

    private IMapBackgroundRenderer GetMapBackgroundRenderer()
    {
        return _presentationMode == PresentationMode.Real
            ? _realMapRenderer
            : _debugMapRenderer;
    }

    private enum MapControlState
    {
        Normal = 0,
        Dialogue = 1,
        Transitioning = 2
    }
}