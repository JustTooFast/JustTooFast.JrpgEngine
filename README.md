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

**Slice 4.5: Map State Variants + Dynamic Map Presentation (Complete)**

The engine can currently:

- Load game definitions from external JSON data
- Start a new game from a title screen
- Load multiple maps
- Move the party leader on the map
- Block movement using tile collision
- Interact with NPCs on the map
- Display dialogue conversations
- Pause the game using a map menu
- Use story flags to track world state
- Change dialogue based on flag conditions
- Open treasure chests and add items to inventory
- Unlock and open doors using key flags
- Pass through conditional gates once flags are met
- Transition between maps using exit objects
- Dynamically change map presentation based on world state
- Enable and disable map objects based on story flags

Movement remains **tile-based for gameplay** but **smoothly interpolated visually**.

Slice 4 expands the gameplay interaction loop:

player → interaction → dialogue → results → world state changes → map transitions

Slice 4.5 extends this loop by allowing **map state to react to world flags** without scripting.

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

data/
- game/
- maps/
- characters/
- dialogues/
- interactions/
- items/

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
-   spawns

Blocked tiles prevent movement.

Map objects allow placing interactive entities such as:

-   NPCs
-   treasure chests
-   locked doors
-   conditional gates
-   map exits

The runtime representation is handled by:

MapRuntime
MapCollisionService

NPC tiles are treated as blocked tiles so players must stand adjacent to
interact.

------------------------------------------------------------------------

## Map State Variants

Slice 4.5 introduces **map state variants**, allowing maps to react to
story progression without scripting.

A map can define optional **state variants** that become active when
specific story flags are set.

A variant can:

- change the map's visual style
- enable objects that were previously hidden
- disable objects that should no longer appear

Only **one variant may be active at a time**.

Variants are resolved whenever:

- a map loads
- a story flag changes

The engine rebuilds the map runtime state deterministically using the
current story flags.

This allows environmental changes such as:

- lights turning on
- generators activating
- objects appearing or disappearing
- progression-based world changes

### Object Visibility Conditions

Individual map objects can optionally define simple visibility rules.

Supported conditions:

- `VisibleIfFlagSet`
- `VisibleIfFlagClear`

Only one condition may be defined per object.

Examples:

- an NPC appearing after a quest begins
- an object disappearing after it is used
- world changes tied to story progression

### Visual Style Changes

Map variants may define a `VisualStyleId`.

This identifier allows the renderer to change how the map is drawn.

In the current debug renderer this affects:

- floor color

Example uses include:

- lights turning on in a room
- environmental alert states
- world power systems activating

------------------------------------------------------------------------

## Player Movement

Movement rules:

-   4 directions only
-   One tile at a time
-   No diagonal movement
-   Movement blocked by collision tiles or objects
-   Facing direction updates even if movement fails
-   Held input repeats movement with a delay

Gameplay position is tile-based:

PlayerTileX
PlayerTileY

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

Interactions use **front‑tile interaction**, with a small exception:

Non‑blocking exit tiles may also be activated while standing on them.

Interaction rules:

-   Only the tile directly in front of the player is checked
-   No diagonal interaction
-   Only one object may be interacted with at a time
-   Interaction is triggered using the **Interact key**

Objects reference interaction definitions which determine the behavior
to run.

Interaction types implemented:

-   NPC dialogue
-   treasure chests
-   locked doors
-   flag gates
-   map exits

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

DialogueSession
DialogueOverlay

### Dialogue Variants

Dialogue can change depending on story flags.

Example conditions:

-   `hasFlag`
-   `lacksFlag`

This allows NPC dialogue to change after the player performs an action
or after a conversation has already occurred.

### Dialogue Results

Dialogue variants can apply results when the conversation finishes.

Supported result types:

-   `SetFlag`
-   `GiveItem`

Results are executed **after the final dialogue line completes**.

------------------------------------------------------------------------

## Story Flags

Story flags represent persistent world state such as:

-   NPC conversations completed
-   treasure chests opened
-   doors unlocked
-   quest progress
-   story events

Runtime system:

StoryFlagState

------------------------------------------------------------------------

## Inventory System

The inventory system supports:

-   storing items
-   adding items from interactions
-   checking if items exist

This slice intentionally avoids:

-   equipment
-   stack limits
-   item usage
-   crafting

These systems will be added in later slices.

Runtime system:

InventoryState

------------------------------------------------------------------------

## Locked Doors

Locked doors now have a **two‑step state model**.

Door behavior:

1.  Player interacts with door
2.  If required flag (key) is missing → locked dialogue
3.  If required flag is present → unlock dialogue plays
4.  Door sets its own open flag
5.  Door tile becomes passable

This separates:

-   *having the key*
-   *door being opened*

------------------------------------------------------------------------

## Flag Gates

Flag gates are conditional blockers.

Behavior:

-   Block movement until a required flag is set
-   Once the flag is set, the gate becomes passable immediately

Unlike locked doors, no interaction is required once the condition is
met.

------------------------------------------------------------------------

## Map Transitions

Slice 4 introduces **multi‑map support**.

Map exits allow transitions between maps.

Exit behavior:

-   stepping onto an exit tile or interacting with it
-   engine loads destination map
-   player appears at a defined spawn location
-   facing direction is restored from the spawn definition

Maps define spawn points used as entry positions.

------------------------------------------------------------------------

## Rendering (Debug)

Rendering systems:

MapRenderer
PlayerRenderer
NpcRenderer
DialogueOverlay

Debug visuals currently render:

-   map tiles
-   blocked tiles
-   NPCs
-   chests
-   doors / gates
-   map exits
-   player
-   dialogue box

Rendering contains **no gameplay logic**.

------------------------------------------------------------------------

# Controls

| Key | Action |
|-----|-------|
| Arrow Keys / WASD | Move |
| Enter / Space | Interact / Advance Dialogue |
| Enter | Start game (title screen) |
| Escape | Open / close map menu |
| Escape (Title Screen) | Exit application |

Movement supports **hold‑to‑walk** behavior.

------------------------------------------------------------------------

# Project Structure

src/JrpgEngine

Key directories:

- Core -- Engine coordination
- Definitions -- Data definitions
- Dialogue -- Dialogue runtime + UI
- Interactions -- Map interaction systems
- Maps -- Map runtime + movement
- Menus -- Map menu overlay
- Rendering -- Debug renderers
- Scenes -- Title and map scenes
- State -- Game runtime state
- Systems -- Engine services

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

- Slice 0 -- Bootstrap
- Slice 1 -- Map navigation foundation
- Slice 2 -- Map interaction and dialogue system
- Slice 3 -- Stateful interactions, flags, and inventory
- Slice 4 -- Map transitions, doors, and conditional gates
- Slice 4.5 -- Map state variants and dynamic presentation

Planned next slices:

- Slice 5 -- Battle system foundation

------------------------------------------------------------------------

# License

Apache License 2.0