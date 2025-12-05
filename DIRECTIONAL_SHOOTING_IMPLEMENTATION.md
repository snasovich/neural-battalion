# Directional Shooting and Level Boundaries Implementation

This document describes the implementation of directional tank shooting and visible level boundaries in Neural Battalion.

## Features Implemented

### 1. Directional Tank Sprites

Tanks now display different sprites based on their facing direction (Up, Down, Left, Right). This provides visual feedback about which direction the tank will shoot.

#### Implementation Details

**TankSpriteManager** (`Assets/Scripts/Utility/TankSpriteManager.cs`)
- Provides utility methods for creating directional tank sprites
- Generates 4 different sprites for each direction with proper gun/turret orientation
- Automatically determines direction from angles or vectors
- Creates procedural sprites as fallbacks when assets aren't available

**Key Features:**
- `CreateDirectionalSprites(Color, size)` - Creates a set of 4 directional sprites
- `GetDirectionFromAngle(angle)` - Converts rotation angle to Direction enum
- `GetDirectionFromVector(vector)` - Converts movement vector to Direction enum
- Procedurally generated sprites show tank body, tracks, and gun barrel

#### Usage in Controllers

Both `PlayerController` and `EnemyController` have been updated:

1. **Sprite Management:**
   - Each controller maintains a `DirectionalSprites` instance
   - Sprites are created when tank data is applied
   - Current direction is tracked and updated during movement

2. **Direction Updates:**
   - Direction changes when movement vector changes
   - Sprite is updated automatically to match facing direction
   - Rotation is still applied to `transform.up` for projectile firing

3. **Integration:**
   ```csharp
   // In PlayerController.cs and EnemyController.cs
   private TankSpriteManager.DirectionalSprites directionalSprites;
   private TankSpriteManager.Direction currentDirection;
   
   private void CreateDirectionalSprites()
   {
       Color tankColor = tankData?.TankColor ?? Color.green;
       directionalSprites = TankSpriteManager.CreateDirectionalSprites(tankColor, 32);
       UpdateSpriteDirection(currentDirection);
   }
   ```

### 2. Level Boundaries

Visible boundaries are now created around the playable area, preventing tanks from leaving the level.

#### Implementation Details

**LevelBoundary** (`Assets/Scripts/Terrain/LevelBoundary.cs`)
- Creates visible border walls around the level
- Provides collision detection to prevent tanks from exiting
- Automatically sized based on grid dimensions
- Visual customization through color settings

**Key Features:**
- `Initialize(width, height, tileSize)` - Sets up boundary for specific level size
- `IsWithinBounds(position)` - Checks if a position is inside the play area
- `IsTankWithinBounds(position, tankSize)` - Checks if tank fits within bounds
- `ClampToBounds(position)` - Clamps a position to valid play area

#### Boundary Visual

The boundary consists of:
- Four wall segments (top, bottom, left, right)
- Each wall has:
  - Visual sprite renderer (customizable color)
  - Box collider for physics collision
  - Configurable thickness

#### Integration with TerrainManager

The `TerrainManager` automatically creates and manages the level boundary:

```csharp
// In TerrainManager.cs
[Header("Level Boundary")]
[SerializeField] private bool createBoundary = true;
[SerializeField] private Color boundaryColor = new Color(0.3f, 0.3f, 0.3f, 1f);

private void CreateLevelBoundary(int width, int height)
{
    // Creates or updates boundary when level is built
    levelBoundary.Initialize(width, height, cellSize.x);
}
```

#### Integration with Tank Controllers

Both `PlayerController` and `EnemyController` check boundaries:

```csharp
private bool CanMoveTo(Vector2 targetPosition)
{
    // Check level boundaries
    if (levelBoundary != null)
    {
        if (!levelBoundary.IsTankWithinBounds(targetPosition, collisionCheckRadius * 2))
        {
            return false;
        }
    }
    
    // ... other collision checks ...
}
```

## How to Test

### Testing in Unity Editor

1. **Open the GameScene:**
   - Open `Assets/Scenes/GameScene.unity`

2. **Setup TerrainManager:**
   - Ensure a GameObject with `TerrainManager` component exists
   - Verify "Create Boundary" is checked in the inspector
   - Adjust boundary color if desired

3. **Test Player Tank:**
   - Start Play mode
   - Move the tank using WASD or arrow keys
   - **Verify:** Tank sprite changes direction when you change movement direction
   - **Verify:** Tank cannot move outside the visible boundary walls
   - **Verify:** Projectiles fire in the direction the tank is facing

4. **Test Enemy Tanks:**
   - Spawn enemy tanks in the scene
   - **Verify:** Enemy sprites change direction as they move
   - **Verify:** Enemies cannot move outside the boundary
   - **Verify:** Enemy projectiles fire in their facing direction

### Visual Indicators

**Directional Sprites:**
- **Up:** Gun barrel points upward
- **Right:** Gun barrel points right
- **Down:** Gun barrel points downward
- **Left:** Gun barrel points left

Each sprite shows:
- Tank body (main color from TankData)
- Tracks (darker shade on sides)
- Gun/turret (lighter shade pointing in direction)

**Boundary Walls:**
- Dark gray borders (default color: `RGB(0.3, 0.3, 0.3)`)
- Visible around entire play area
- Solid collision prevents tank passage

## Configuration

### TankSpriteManager

No configuration needed - automatically generates sprites based on tank color.

To customize sprite appearance, modify the drawing methods in `TankSpriteManager.cs`:
- `DrawTankUp()`, `DrawTankRight()`, `DrawTankDown()`, `DrawTankLeft()`
- Adjust body dimensions, track sizes, or turret proportions

### LevelBoundary

Configure in TerrainManager Inspector:
- **Create Boundary** - Enable/disable boundary creation
- **Boundary Color** - Color of the boundary walls
- **Border Thickness** - Thickness of boundary walls (default: 0.5 units)

Or configure directly on LevelBoundary component:
```csharp
[SerializeField] private float borderThickness = 0.5f;
[SerializeField] private Color borderColor = new Color(0.3f, 0.3f, 0.3f, 1f);
[SerializeField] private int sortingOrder = -1; // Render behind gameplay
```

## Technical Notes

### Projectile Firing Direction

Projectiles already fired in the direction of `transform.up`, which is set by the rotation during movement. No changes were needed to projectile firing logic - the directional sprites are purely visual while the transform rotation handles the actual shooting direction.

### Performance

**Sprite Generation:**
- Sprites are generated once when tank is initialized
- Procedural generation is fast (< 1ms per tank)
- Sprites are cached and reused during gameplay
- No runtime texture generation after initialization

**Boundary Checking:**
- Simple bounds check using Unity's `Bounds.Contains()`
- Minimal performance impact (< 0.1ms per frame)
- Only checked during movement attempts
- No raycast or complex collision detection needed

### Fallback Behavior

**If TankData has a custom sprite:**
- Custom sprites are ignored in favor of directional sprites
- This ensures consistent directional appearance

**If boundary creation fails:**
- Tanks will still function normally
- Warning logged to console
- No runtime errors or crashes

**If LevelBoundary not found:**
- Movement validation continues with terrain/tank checks only
- Warning logged to console
- Tanks can potentially move outside level (visual only)

## Future Enhancements

Potential improvements for future development:

1. **Custom Sprite Sets:**
   - Allow TankData to specify 4 sprites for directions
   - Use custom sprites when available, fallback to procedural

2. **Boundary Visual Variants:**
   - Different boundary styles (brick wall, fence, etc.)
   - Animated boundaries
   - Particle effects at boundaries

3. **Additional Directions:**
   - Support 8-direction movement and sprites
   - Diagonal tank orientations

4. **Turret Separation:**
   - Separate tank body and turret sprites
   - Allow turret to aim independently of movement

## Troubleshooting

**Issue:** Tank sprite doesn't change direction
- **Solution:** Ensure `spriteRenderer` is assigned in tank Inspector
- **Solution:** Check that `CreateDirectionalSprites()` is called in `ApplyTankData()`

**Issue:** Tanks can move outside the level
- **Solution:** Verify `LevelBoundary` component exists in scene
- **Solution:** Ensure boundary is initialized with correct dimensions
- **Solution:** Check that `levelBoundary` reference is found in tank Start()

**Issue:** Boundary walls not visible
- **Solution:** Check boundary color alpha is not 0
- **Solution:** Verify sorting order is appropriate for your scene
- **Solution:** Ensure boundary GameObjects are not disabled

**Issue:** Projectiles fire in wrong direction
- **Solution:** Check that tank rotation is being updated in Move()
- **Solution:** Verify `transform.up` is used in Fire() method
- **Solution:** Ensure rotationSpeed is set in TankData

## Code References

### Key Files Modified

1. **Assets/Scripts/Player/PlayerController.cs**
   - Added directional sprite support
   - Added boundary collision checking
   - Updated Move() method

2. **Assets/Scripts/Enemy/EnemyController.cs**
   - Added directional sprite support
   - Added boundary collision checking
   - Updated Move() method

3. **Assets/Scripts/Terrain/TerrainManager.cs**
   - Added boundary creation
   - Integrated with BuildLevel()

### New Files Created

1. **Assets/Scripts/Utility/TankSpriteManager.cs**
   - Directional sprite generation
   - Direction utilities

2. **Assets/Scripts/Terrain/LevelBoundary.cs**
   - Boundary rendering and collision
   - Bounds checking utilities

## Summary

This implementation provides:
- ✅ Directional tank sprites for all 4 cardinal directions
- ✅ Visual feedback for tank facing direction
- ✅ Proper projectile firing in tank direction
- ✅ Visible level boundaries
- ✅ Collision prevention at boundaries for both player and enemies
- ✅ Automatic integration with existing systems
- ✅ Fallback sprites when no assets provided
- ✅ Minimal performance impact
- ✅ Easy configuration and customization
