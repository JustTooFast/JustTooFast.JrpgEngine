# JustTooFast.JrpgEngine

Slice 0 bootstrap for a JRPG engine using:

- VS Code
- MonoGame
- Tiled

## Slice 0 includes

- MonoGame DesktopGL project
- VS Code build/debug setup
- Content pipeline present
- Tiled asset folders present
- Sample TMX and tileset committed

## Slice 0 does not include

- gameplay code
- scene system
- input abstraction
- data loading
- runtime state
- map logic
- collision
- UI/menu systems

## Run

```bash
dotnet build JustTooFast.JrpgEngine.slnx
dotnet run --project src/JrpgEngine/JrpgEngine.csproj