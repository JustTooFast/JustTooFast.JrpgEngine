# JustTooFast.JrpgEngine

A **data-driven 2D JRPG engine** built in C# using **MonoGame**.

The repository contains:

- **JrpgEngine** — the reusable JRPG engine library
- **JrpgGame** — a playable host application using the engine

The engine is developed incrementally using **working vertical slices**.
Each slice adds playable functionality while maintaining strict
architectural separation between:

-   definitions (data)
-   runtime state
-   systems
-   scenes
-   rendering

This approach keeps the engine stable while gameplay systems are added.

The engine currently supports classic JRPG presentation techniques such
as **foreground / overhead map layers** and **directional player sprite
rendering**, allowing characters to walk behind environmental elements
like pillars, arches, tree canopies, and roof overhangs.

------------------------------------------------------------------------

# Design Philosophy

This engine is designed around a few core principles intended to keep
JRPG gameplay systems predictable, maintainable, and easy to extend.

### Data-Driven Story Content

Story and gameplay content are defined through external data rather
than engine code.

Maps, interactions, dialogue, and items are loaded from JSON files
located in the `/data` directory.

This allows game content to be created without modifying engine logic.

### Explicit Gameplay Systems

Gameplay rules live inside engine systems rather than in scripting
languages.

This makes behavior easier to reason about and avoids hidden logic that
can emerge in script-heavy engines.

### Deterministic World State

The game world is rebuilt from:

- definitions
- runtime state
- story flags

This ensures that map state, object visibility, and interactions always
produce consistent results.

### Rendering Is Separate From Gameplay

Rendering components are treated as **views**.

They read runtime state and draw the world but contain **no gameplay
logic**.

This allows presentation to evolve independently from gameplay systems.

### Incremental Vertical Slices

The engine is developed through **working vertical slices**.

Each slice:

- compiles
- runs
- can be tested in-game
- integrates with existing systems
- avoids speculative architecture

This approach keeps the engine playable throughout development and
prevents unfinished systems from accumulating.

------------------------------------------------------------------------

# Engine Architecture Overview

The engine is organized into a small number of explicit layers.

Definitions → Runtime State → Systems → Scenes → Rendering

### Definitions

Definitions are immutable data loaded from external JSON files.

Examples include:

- maps
- interactions
- dialogues
- items
- characters
- game configuration

Definitions describe what exists in the game, but do not contain runtime
behavior.

### Runtime State

Runtime state stores the current play session.

Examples include:

- current map
- player position
- facing direction
- story flags
- inventory
- active transitions

Runtime state changes as the player plays the game.

### Systems

Systems contain gameplay rules and state transition logic.

Examples include:

- movement
- collision checks
- interaction execution
- dialogue result application
- map state resolution

Systems read definitions and runtime state, then apply gameplay rules in
a deterministic way.

### Scenes

Scenes coordinate gameplay flow and input handling.

Examples include:

- TitleScene
- MapScene

Scenes decide which systems run, when updates occur, and which view
components should draw.

### Rendering

Rendering components draw the current runtime state.

Examples include:

- DebugMapRenderer
- RealMapRenderer
- MapObjectRenderer
- PlayerRenderer
- DialogueOverlay

Rendering is presentation-only and contains no gameplay logic.

### Layer Responsibility

A useful rule of thumb is:

- **Definitions** describe the world
- **Runtime state** stores the current world state
- **Systems** apply gameplay rules
- **Scenes** coordinate update and draw flow
- **Rendering** displays the result

This separation keeps gameplay logic explicit and allows presentation to
change without affecting core game behavior.

------------------------------------------------------------------------

# Engine / Game Host Separation

Starting with **Slice 4.9**, the project is split into two projects:

- **JrpgEngine** — reusable engine library
- **JrpgGame** — executable game host

This separation keeps the engine reusable while allowing individual
games to provide their own assets and startup configuration.

### JrpgEngine (Library)

The engine library contains all reusable gameplay systems:

- definitions and definition loading
- runtime state models
- gameplay systems
- map systems
- interaction systems
- dialogue systems
- rendering systems
- scene management

The engine contains **no executable entry point** and does not own
content assets.

### JrpgGame (Executable Host)

The game host provides the runnable application and concrete assets.

Responsibilities include:

- Program entry point
- MonoGame Game bootstrap
- window and application configuration
- MonoGame Content pipeline
- concrete visual assets

The host launches the engine and supplies content and configuration.

### Why This Split Exists

Separating the engine from the game host allows:

- reuse of the engine in multiple projects
- clean separation between **engine code** and **game content**
- clearer ownership of assets and application startup
- easier experimentation with different games using the same engine

Gameplay rules and systems remain inside **JrpgEngine**, while
the executable application and assets live inside **JrpgGame**.

------------------------------------------------------------------------

# Current Status

**Slice 5.3: Real Map Object Rendering + Shared Visual Definitions — Complete**

The engine now supports **real visual rendering for map objects** in addition to
player sprite rendering and map visuals.

Map objects such as NPCs, chests, doors, and exits can now reference shared
visual definitions, allowing both characters and environmental objects to
render using the same sprite rendering system.

Visual definitions describe how a sprite sheet should be interpreted,
including frame size and directional layout. This allows different game
entities to share a consistent visual contract while remaining data-driven.

The engine now supports:

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
- Render maps using a development debug renderer
- Render maps using real visual assets through the MonoGame content pipeline
- Render optional overhead visual layers above gameplay actors
- Render the player as a real 4-direction sprite in Real presentation mode
- Render map objects using shared visual definitions
- Trigger random encounters based on player movement
- Transition between exploration and battle scenes

Movement remains **tile-based for gameplay** but **smoothly interpolated visually**.

The current gameplay loop is:

player → exploration → encounter → battle → return → exploration

------------------------------------------------------------------------

# Features Implemented

## Boot + Title Flow

Startup sequence:

GameRoot → TitleScene → MapScene → BattleScene → MapScene

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

- NPCs
- treasure chests
- locked doors
- conditional gates
- map exits
- other environmental interactables

Map objects are defined using **two layers**:

**MapObjectDef**

Reusable definition describing the object type and visual.

**MapObjectPlacementDef**

Placement of a specific object instance on a map.

Example placement fields include:

- `id`
- `mapObjectDefId`
- `x`
- `y`
- `interactionId`
- visibility conditions

This separation allows many maps to reuse the same object definitions
while controlling placement and behavior through data.

Map objects may optionally reference **visual definitions**, allowing
them to render real sprites in Real presentation mode while still
appearing as simplified shapes in Debug presentation mode.

The runtime representation is handled by:

MapRuntime
MapCollisionService

NPC tiles are treated as blocked tiles so players must stand adjacent to
interact.

### Map Visual Assets

Maps may optionally define visual assets used by the real renderer.

Supported fields:

- `visualAssetId`
- `overheadVisualAssetId`

The base visual asset represents the map background.

The optional overhead visual asset represents environmental elements
that should render above gameplay actors.

Both assets are loaded through the MonoGame content pipeline.

### Visual Definitions

Slice 5.3 introduces **visual definitions**, which describe how a sprite
sheet should be interpreted by the renderer.

Visual definitions are shared by both:

- characters
- map objects

A visual definition contains information such as:

- `visualAssetId`
- `frameWidth`
- `frameHeight`
- `frameCount`
- `facingDirections`

This allows the renderer to determine which portion of a texture should
be drawn.

Example visual definition:

```json
{
  "id": "visual_npc_scientist",
  "visualAssetId": "Sprites/npc_scientist",
  "frameWidth": 32,
  "frameHeight": 32,
  "frameCount": 1,
  "facingDirections": 1
}
```

Because visuals are defined independently from gameplay objects,
multiple gameplay entities can share the same visual definition.

This design keeps rendering data-driven while maintaining a clear
separation between gameplay logic and presentation.

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

### Visual Asset Changes

Map variants may define a `VisualAssetOverrideId`.

This identifier allows a variant to override the base visual asset of the
map.

The renderer uses the already-resolved runtime map state to determine
which visual asset should be drawn.

This means map presentation can change when story flags change, while
gameplay logic remains unchanged.

Example uses include:

- lights turning on in a room
- environmental state changes
- progression-based map presentation updates
- world power systems activating

In debug presentation mode, the engine continues to use debug map
rendering for development visibility.

In real presentation mode, the engine draws the resolved map visual
asset through the MonoGame content pipeline.

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

- random encounter checks
- poison damage
- environmental hazards
- scripted triggers

------------------------------------------------------------------------

## Random Encounter System

Slice 5 introduces **random encounters triggered by player movement**.

Encounters are evaluated whenever the player successfully completes a
tile movement.

Encounter rules:

- Only successful tile movement counts as a step
- Blocked movement does not count
- Turning in place does not count
- Encounter checks occur after step-trigger interactions

Maps may optionally define encounter configuration:

- `EncountersEnabled`
- `EncounterRate`
- `EncounterTableId`

Example map configuration:

```json
{
"encountersEnabled": true,
"encounterRate": 12,
"encounterTableId": "encounter_table_field"
}
```

Encounter tables define the possible enemy groups that can appear.

Example encounter table:

```json
{
"id": "encounter_table_field",
"entries": [
   { "encounterId": "encounter_slime_x2", "weight": 100 }
]
}
```

The engine resolves encounters using weighted random selection.

### Encounter Flow

The encounter pipeline works as follows:

movement step  
→ encounter check  
→ encounter table selection  
→ BattleScene created  
→ battle resolves  
→ return to MapScene

Battle logic itself will be implemented in later slices.

For Slice 5, the battle scene currently acts as a minimal placeholder
that demonstrates the exploration → battle → return pipeline.

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

## Rendering

Rendering is intentionally separated from gameplay systems.

Gameplay logic lives in engine systems while rendering is handled by
view-only components.

Current renderers include:

DebugMapBackgroundRenderer  
RealMapBackgroundRenderer  
MapBackgroundRenderer  
MapObjectRenderer  
DebugMapObjectRenderer  
RealMapObjectRenderer  
DebugPlayerRenderer  
RealPlayerRenderer  
PlayerRenderer  
MapOverheadRenderer  
DialogueOverlay

Rendering systems contain **no gameplay logic** and operate only on
runtime state.

### Debug Map Rendering

Debug rendering visualizes gameplay structure rather than final visuals.

Debug mode renders:

- map tiles
- blocked tiles
- NPCs
- chests
- doors / gates
- map exits
- player
- dialogue box

This mode is used during development to clearly see map structure,
collision, and object placement.

### Real Map Rendering

Slice **4.75** introduced the first real map renderer.

Maps may reference visual assets loaded through the MonoGame content
pipeline.

These assets are rendered as map backgrounds while gameplay actors
continue to render above them.

### Overhead Visual Layers

Slice **5.1** adds support for optional **overhead map visuals**.

A map may define an additional visual asset:

`overheadVisualAssetId`

This asset is rendered **after the player and map objects**, allowing
environmental features to appear above gameplay actors.

Typical uses include:

- tree canopies
- bridge tops
- cave ceilings
- roof overhangs
- architectural details such as pillars or arches

Overhead visuals are **purely presentation** and do not affect gameplay
systems such as:

- collision
- interaction detection
- object visibility

Maps that do not define an overhead asset render exactly as before.

### Player Sprite Rendering

Slice **5.2** adds real player sprite rendering in **Real** presentation
mode while preserving the original debug player marker in **Debug**
presentation mode.

The player sprite uses a directional sprite sheet layout with:

- 4 rows for directions
- 3 columns reserved for animation frames

Current direction mapping:

- Row 0 → Down
- Row 1 → Left
- Row 2 → Right
- Row 3 → Up

For Slice 5.2 only the first frame column is used.

The sprite sheet contract currently assumes:

- FrameWidth = 32
- FrameHeight = 32
- FramesPerDirection = 3

This produces a full sheet size of:

- Width = 96
- Height = 128

Character definitions reference the player map sprite through:

`visualAssetId`

Example:

    {
      "id": "char_hero",
      "name": "Hero",
      "visualAssetId": "Sprites/debug_player_orientation"
    }

The player shown on the map is still the **party leader**.

### Map Object Rendering

Slice 5.3 introduces **real map object rendering**.

Map objects can now render real visual sprites using shared visual
definitions.

Rendering behavior:

Debug presentation mode:

- NPCs appear as blue markers
- chests appear as brown boxes
- gates and doors appear as red blockers
- exits appear as purple markers

This visualization highlights gameplay structure.

Real presentation mode:

- map objects render their configured sprite
- visuals are resolved through `VisualDef`
- the renderer extracts the correct sprite frame from the texture

This system allows the same rendering pipeline used by player sprites to
be reused for map objects, while still keeping debug visualization
useful during development.

### Presentation Modes

The engine supports two presentation modes:

Debug  
Real

Debug mode visualizes gameplay structure such as collision and object
placement.

Real mode renders authored map visuals through the MonoGame content
pipeline and renders the player as a directional sprite.

Overhead visuals are only rendered in **Real** presentation mode.

### Render Order

The layered map draw flow is:

1. Map background
2. Map objects
3. Player
4. Map overhead

This keeps map presentation isolated from gameplay logic while allowing
the player to move visually behind overhead scenery.

------------------------------------------------------------------------

## Player Sprite Content

Concrete sprite assets belong to the **JrpgGame** host project and are
loaded through the MonoGame content pipeline.

Typical location:

`src/JrpgGame/Content/Sprites`

Character map visuals are referenced from character data using
`visualAssetId`.

Example:

    {
      "id": "char_hero",
      "name": "Hero",
      "visualAssetId": "Sprites/debug_player_orientation"
    }

The asset path in JSON should match the MonoGame content asset name and
should not include the `.png` extension.

A development-oriented diagnostic sprite sheet such as
`debug_player_orientation.png` can be useful for verifying:

- facing-direction row mapping
- frame selection
- sprite alignment
- future animation frame indexing

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

The repository now contains two main projects:

src/JrpgEngine  
Reusable engine library containing gameplay systems.

src/JrpgGame  
Executable MonoGame host that launches the engine.

### JrpgEngine Structure

Key directories:

- Core -- Engine coordination
- Definitions -- Data definitions
- Dialogue -- Dialogue runtime + UI
- Interactions -- Map interaction systems
- Maps -- Map runtime + movement
- Menus -- Map menu overlay
- Rendering -- Map, player, and object renderers
- Scenes -- Title and map scenes
- State -- Game runtime state
- Systems -- Engine services

### JrpgGame Structure

The game host contains:

- Program.cs -- executable entry point
- GameRoot -- MonoGame application host
- Content/ -- MonoGame content pipeline assets

Game data is stored outside the project:

data/

This directory contains JSON definitions for:

- maps
- characters
- dialogues
- interactions
- items
- game configuration

Visual assets used by the game are stored in:

src/JrpgGame/Content

This directory contains assets processed by the MonoGame content
pipeline, including:

- map visual assets
- overhead visual assets
- player sprite sheets
- fonts
- other presentation assets

The engine loads these assets through the MonoGame content pipeline,
but the assets themselves belong to the **game host project**, not the
engine library.

------------------------------------------------------------------------

# Running the Engine

From the repository root:

dotnet run --project src/JrpgGame/JrpgGame.csproj

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
- Slice 4.75 -- Map presentation cleanup and real map renderer
- Slice 4.9 -- Engine / Game host separation
- Slice 5 -- Random encounter system
- Slice 5.1 -- Overhead map visual layers
- Slice 5.2 -- Real player sprite rendering (4-direction, non-animated)
- Slice 5.3 -- Real map object rendering and shared visual definitions

------------------------------------------------------------------------

# Example Gameplay Scenario

The current engine slices already support small gameplay scenarios built
entirely through data definitions.

Example: **Power Generator Puzzle**

The player enters a facility where the lights are off.

1. The map loads using a **dark visual variant** because the
   `flag_generator_on` story flag is not set.

2. The player explores the area and finds a **generator console**.

3. Interacting with the console triggers dialogue which sets:

   SetFlag → `flag_generator_on`

4. When the flag is set:

   - the active **map variant changes**
   - the map visual asset switches to the **lit version**
   - new map exits or objects can appear

5. The player can now access areas that were previously unavailable.

This entire sequence is defined through:

- map definitions
- interaction definitions
- dialogue results
- story flags
- map state variants

No engine code changes are required to implement the puzzle.

This demonstrates the engine's design goal of supporting **story-driven
world changes through data rather than scripting**.

------------------------------------------------------------------------

# License

Apache License 2.0