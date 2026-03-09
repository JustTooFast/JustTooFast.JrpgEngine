# JustTooFast.JrpgEngine

A **data-driven 2D JRPG engine** built in C# using **MonoGame**.

The engine is developed incrementally using **working vertical slices**.
Each slice adds playable functionality while maintaining strict
architectural separation between:

-   definitions (data)
-   runtime state
-   systems
-   scenes
-   rendering

This approach keeps the engine stable while gameplay systems are added.

------------------------------------------------------------------------

# Current Status

**Slice 2: Map Interaction + Dialogue (Complete)**

The engine can currently:

-   Load game definitions from external JSON data
-   Start a new game from a title screen
-   Load a test map
-   Move the party leader on the map
-   Block movement using tile collision
-   Interact with NPCs on the map
-   Display dialogue conversations
-   Render NPCs and dialogue UI
-   Pause the game using a map menu

Movement remains **tile-based for gameplay** but **smoothly interpolated
visually**.

Slice 2 introduces the first gameplay interaction loop on the map:

player → interaction → dialogue → return to map control

------------------------------------------------------------------------

# Features Implemented

## Boot + Title Flow

Startup sequence:

GameRoot → TitleScene → MapScene

The title screen allows starting a new game and exiting the application.

------------------------------------------------------------------------

## Data‑Driven Definitions

Game content is loaded from JSON files located in the `/data` directory.

Example structure:

data/ game/ maps/ characters/ dialogues/ interactions/

Definitions are loaded during engine startup through:

DefinitionLoader

Definitions are immutable data used to construct runtime state.

------------------------------------------------------------------------

## Map System

Maps are defined as tile grids.

Example map definition fields:

-   id
-   width
-   height
-   tileSize
-   blockedTiles
-   objects

Blocked tiles prevent movement.

Map objects allow placing interactive entities such as NPCs.

The runtime representation is handled by:

MapRuntime MapCollisionService

NPC tiles are treated as blocked tiles so players must stand adjacent to
interact.

------------------------------------------------------------------------

## Player Movement

Movement rules:

-   4 directions only
-   One tile at a time
-   No diagonal movement
-   Movement blocked by collision tiles or NPCs
-   Facing direction updates even if movement fails
-   Held input repeats movement with a delay

Gameplay position is tile-based:

PlayerTileX PlayerTileY

Visual movement is interpolated between tiles using:

PlayerMapMover

Tile movement commit points allow later systems to hook into movement
events such as:

-   encounter steps
-   poison damage
-   environmental hazards
-   scripted triggers

------------------------------------------------------------------------

## Map Interaction System

Slice 2 introduces **front‑tile interaction**.

Interaction rules:

-   Only the tile directly in front of the player is checked
-   No diagonal interaction
-   Only one object may be interacted with at a time
-   Interaction is triggered using the **Interact key**

Objects reference interaction definitions which determine the behavior
to run.

Example interaction types planned:

-   NPC dialogue
-   treasure chests
-   map transitions
-   scripted events

Slice 2 implements **NPC dialogue only**.

------------------------------------------------------------------------

## NPC Rendering

NPCs placed in map definitions are rendered on the map using a simple
debug renderer.

Renderer:

NpcRenderer

NPCs currently render as debug colored tiles similar to the player.

Rendering is intentionally separated from gameplay logic.

------------------------------------------------------------------------

## Dialogue System

The dialogue system allows NPCs to present multi‑line conversations.

Behavior:

-   Dialogue begins when interacting with an NPC
-   Dialogue advances with Enter or Space
-   Dialogue closes automatically after the final line

While dialogue is active:

-   Player movement is disabled
-   Map interaction is disabled
-   Map menu cannot open

Systems involved:

DialogueSession DialogueOverlay

Dialogue content is loaded from JSON definitions.

------------------------------------------------------------------------

## Map Menu

Press Escape while on the map to open the map menu.

Menu options:

-   Resume
-   Return to Title

Rules:

-   Menu opens only while on the map
-   Menu cannot open during dialogue
-   Closing the menu returns control to the map
-   Menu input does not trigger map interactions

The menu is implemented as an overlay system rather than a scene.

------------------------------------------------------------------------

## Rendering (Debug)

Rendering systems:

MapRenderer PlayerRenderer NpcRenderer DialogueOverlay

Debug visuals currently render:

-   map tiles
-   blocked tiles
-   NPCs
-   player
-   dialogue box

Rendering contains **no gameplay logic**.

------------------------------------------------------------------------

# Controls

  Key                     Action
  ----------------------- -----------------------------
  Arrow Keys / WASD       Move
  Enter / Space           Interact / Advance Dialogue
  Enter                   Start game (title screen)
  Escape                  Open / close map menu
  Escape (Title Screen)   Exit application

Movement supports **hold‑to‑walk** behavior.

------------------------------------------------------------------------

# Project Structure

src/JrpgEngine

Key directories:

Core -- Engine coordination
Definitions -- Data definitions
Dialogue -- Dialogue runtime + UI
Interactions -- Map interaction systems
Maps -- Map runtime + movement
Menus -- Map menu overlay
Rendering -- Debug renderers
Scenes -- Title and map scenes
State -- Game runtime state
Systems -- Engine services

Game data is stored outside the project:

data/

------------------------------------------------------------------------

# Running the Engine

From the repository root:

dotnet run --project src/JrpgEngine/JrpgEngine.csproj

------------------------------------------------------------------------

# Development Approach

The engine is built using **incremental vertical slices**.

Each slice:

1.  adds a playable feature
2.  integrates with existing systems
3.  avoids speculative architecture

Completed slices:

Slice 0 -- Bootstrap
Slice 1 -- Map navigation foundation
Slice 2 -- Map interaction and dialogue system

Planned next slices:

Slice 3 -- Map events and interactable objects
Slice 4 -- Battle system foundation

------------------------------------------------------------------------

# License

Apache License 2.0