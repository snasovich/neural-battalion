# Game Level Implementation Notes

## Overview
This implementation adds the ability to load and display game levels in the GameScene, similar to the first level from Battle City.

## Changes Made

### 1. Resources Directory Structure
Created `/Assets/Resources/Levels/` directory to store level data assets that can be loaded at runtime using `Resources.Load()`.

### 2. Level Data Asset (Level1.asset)
Created a Battle City-inspired first level with:
- 26x26 tile grid
- Various brick wall formations creating maze-like pathways
- Steel walls protecting the base at the bottom center
- Water hazards in the middle section
- Player base marker at the bottom
- Enemy spawn points at the top (x: 0, 12, 25; y: 25)
- Player spawn point at the bottom (x: 9, y: 0)

#### Tile Legend
- `.` = Empty ground
- `#` = Brick wall (destructible)
- `@` = Steel wall (indestructible)
- `~` = Water (blocks tanks, bullets pass through)
- `*` = Trees (provides cover)
- `-` = Ice (slippery surface)
- `B` = Base (must be protected)
- `P` = Player spawn point
- `E` = Enemy spawn point

### 3. GameSceneController Script
Created `Assets/Scripts/GameSceneController.cs` to:
- Initialize the GameScene when it loads
- Load level data from Resources/Levels/Level1
- Find and configure the TerrainManager
- Trigger level building

This script should be attached to a GameObject in the GameScene.

### 4. TerrainManager Updates
Updated `BuildLevel()` method in `TerrainManager.cs` to:
- Parse the tile data from LevelData using `ParseTileData()`
- Iterate through the grid and place tiles using `SetTile()`
- Add debug logging for level building process

Added `CreateFallbackTilesIfNeeded()` method to:
- Generate colored tiles programmatically when sprite assets are not assigned
- Use the SpriteGenerator utility to create simple colored sprites
- Each tile type gets a distinct color for visual identification

### 5. SpriteGenerator Utility
Created `Assets/Scripts/Utility/SpriteGenerator.cs` to:
- Generate simple colored sprites at runtime
- Provide a fallback visualization system when art assets are missing
- Create 16x16 pixel sprites with solid colors for each tile type

## How It Works

1. When the MainMenu's Play button is clicked, it loads the "GameScene"
2. GameSceneController.Start() is called automatically
3. GameSceneController loads "Level1" from Resources/Levels/
4. The level data is passed to TerrainManager.BuildLevel()
5. TerrainManager creates fallback tiles if sprite assets are not assigned
6. TerrainManager parses the tile data string into a 2D grid
7. Each tile is placed on the appropriate Tilemap (ground, obstacle, decoration)
8. The level is now visible in the game view

## Current Tile Colors (Fallback)
- Brick: Orange-brown (RGB: 0.8, 0.4, 0.2)
- Steel: Gray (RGB: 0.7, 0.7, 0.7)
- Water: Blue (RGB: 0.2, 0.4, 0.8)
- Trees: Green (RGB: 0.2, 0.6, 0.2)
- Ice: Light blue (RGB: 0.7, 0.9, 1.0)
- Ground: Dark gray (RGB: 0.2, 0.2, 0.2)

## GameScene Structure
The scene now contains:
- **Main Camera**: Positioned at (13, 13, -10) with orthographic size 13 to show the full 26x26 grid
- **GameController**: GameObject with GameSceneController script attached
- **Grid**: GameObject with TerrainManager and Grid components
  - **Ground**: Child Tilemap for ground tiles (water, ice, empty)
  - **Obstacle**: Child Tilemap for obstacle tiles (brick, steel)
  - **Decoration**: Child Tilemap for decoration tiles (trees)

## Next Steps
To improve the visualization:
1. Create proper sprite assets for each tile type in Assets/Art/Sprites/
2. Assign these sprites to the TerrainManager's tile asset fields in the Unity Inspector
3. Add player tank prefab and spawn it at the player spawn point
4. Add enemy spawner system to spawn enemies at enemy spawn points
5. Add base GameObject at the base position
6. Implement level progression (loading next levels after completion)

## Testing
To test in Unity:
1. Open the project in Unity
2. Open Scenes/MainMenu.unity
3. Press Play
4. Click the Play button
5. The GameScene should load and display the level with colored tiles

## File Locations
- Level data: `Assets/Resources/Levels/Level1.asset`
- GameScene controller: `Assets/Scripts/GameSceneController.cs`
- Updated terrain manager: `Assets/Scripts/Terrain/TerrainManager.cs`
- Sprite generator utility: `Assets/Scripts/Utility/SpriteGenerator.cs`
- Game scene: `Assets/Scenes/GameScene.unity`
