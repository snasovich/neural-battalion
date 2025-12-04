# Enemy AI System Documentation

## Overview
The Enemy AI system implements intelligent tank enemies with movement and combat behaviors using a State Machine pattern. This document describes the implementation, components, and usage.

## Architecture

### Components

#### 1. EnemyController.cs
**Purpose**: Manages individual enemy tank behavior, movement, and combat.

**Key Features**:
- Movement in 4 cardinal directions (Battle City style)
- Rotation to face movement direction
- Health management
- Shooting mechanics
- Freeze/unfreeze functionality
- Integration with TankData for configuration

**Public Methods**:
```csharp
void Initialize(int id, int type, TankData data)  // Initialize enemy with configuration
void SetMoveDirection(Vector2 direction)          // Set movement direction (called by AI)
bool TryFire()                                     // Attempt to fire weapon
void TakeDamage(int damage)                       // Receive damage
void Freeze(float duration)                       // Temporarily freeze enemy
void ForceDestroy()                               // Destroy enemy immediately
```

#### 2. EnemyAI.cs
**Purpose**: Implements AI decision-making using a State Machine.

**AI States**:
1. **Idle**: Waiting at spawn point
2. **Patrol**: Random movement pattern
3. **Chase**: Moving toward detected player
4. **Attack**: Firing at player in range

**State Transitions**:
```
      ┌─────────────────────────────────────┐
      │                                     │
      ▼                                     │
 ┌─────────┐    Player in    ┌─────────┐    │
 │  IDLE   │───────────────► │  CHASE  │────┘
 └────┬────┘     range       └────┬────┘
      │                           │
      │ Patrol timer              │ Player in
      ▼                           │ attack range
 ┌─────────┐                      ▼
 │ PATROL  │◄────────────── ┌─────────┐
 └─────────┘   Lost player  │ ATTACK  │
                            └─────────┘
```

**Configuration Parameters**:
- `detectionRange`: Distance to detect player (default: 10 units)
- `attackRange`: Distance to start attacking (default: 8 units)
- `patrolIdleTime`: Time to idle before patrol (default: 2s)
- `directionChangeTime`: How often to change patrol direction (default: 3s)
- `fireChance`: Probability to fire when able (default: 0.3)
- `fireCheckInterval`: Time between fire checks (default: 0.5s)

#### 3. EnemySpawner.cs
**Purpose**: Manages enemy spawning, waves, and active enemy tracking.

**Features**:
- Wave-based spawning
- Multiple spawn points
- Max active enemies limit
- Automatic wave progression
- Enemy type distribution

**Public Methods**:
```csharp
void StartSpawning()                    // Begin spawning waves
void StopSpawning()                     // Stop spawning
void DestroyAllEnemies()                // Destroy all active enemies
void FreezeAllEnemies(float duration)   // Freeze all enemies
List<EnemyController> GetActiveEnemies() // Get list of active enemies
```

#### 4. EnemyTypes.cs
**Purpose**: Defines enemy type constants and properties.

**Enemy Types**:

| Type | Speed | Health | Score | Special Ability |
|------|-------|--------|-------|-----------------|
| Basic | 1.0x | 1 | 100 | None |
| Fast | 1.5x | 1 | 200 | High speed |
| Power | 1.0x | 1 | 300 | Destroys steel |
| Armor | 0.7x | 4 | 400 | High health |

**Helper Methods**:
```csharp
string GetName(int type)              // Get display name
int GetScoreValue(int type)           // Get score value
int GetHealth(int type)               // Get health
float GetSpeedMultiplier(int type)    // Get speed multiplier
bool CanDestroySteel(int type)        // Check steel destruction ability
```

### Data Assets

#### TankData ScriptableObjects
Located in: `Assets/Resources/TankData/`

Files:
- `EnemyBasic.asset` - Basic enemy configuration
- `EnemyFast.asset` - Fast enemy configuration
- `EnemyPower.asset` - Power enemy configuration
- `EnemyArmor.asset` - Armor enemy configuration

Each TankData defines:
- Movement speed
- Rotation speed
- Health
- Fire rate
- Projectile speed
- Damage
- Score value
- Special abilities

#### Wave Data
Located in: `Assets/Resources/Waves/`

`Wave1.asset` defines:
- Total enemies: 10
- Enemy type distribution: Mostly basic, some fast
- Spawn interval: 3 seconds
- Max active enemies: 3

#### Enemy Prefabs
Located in: `Assets/Resources/Prefabs/`

Files:
- `EnemyBasicTank.prefab`
- `EnemyFastTank.prefab`
- `EnemyPowerTank.prefab`
- `EnemyArmorTank.prefab`

Each prefab includes:
- Transform component
- SpriteRenderer (colored sprite based on type)
- Rigidbody2D (2D physics)
- BoxCollider2D (collision detection)
- EnemyController script
- EnemyAI script

## Integration

### GameSceneController Integration
The `GameSceneController` automatically:
1. Loads enemy prefabs from Resources
2. Loads wave data from Resources
3. Creates spawn points if not defined
4. Configures the EnemySpawner
5. Starts enemy spawning

**Spawn Points**:
By default, 3 spawn points are created at the top of the level:
- Left: (-8, 10, 0)
- Center: (0, 10, 0)
- Right: (8, 10, 0)

### Event Integration
The enemy system publishes events through the EventBus:

```csharp
EnemySpawnedEvent      // When enemy spawns
EnemyDestroyedEvent    // When enemy dies
EnemyWaveStartedEvent  // When wave begins
ProjectileFiredEvent   // When enemy fires
```

## Usage

### Basic Setup
1. Ensure GameScene has:
   - TerrainManager component
   - GameSceneController component
   - EnemySpawner component (optional - auto-created)

2. The system will automatically:
   - Load enemy prefabs and data
   - Create spawn points
   - Start spawning enemies

### Testing and Validation
Use the `EnemySystemValidator` script to verify setup:

```csharp
// Attach to a GameObject in the scene
// Validates:
// - TankData assets load correctly
// - Prefabs have required components
// - Wave data is valid
// - Scripts are properly configured
```

### Debugging
Enable debug mode in EnemyAI inspector:
- `debugMode = true` shows detailed state transitions and decisions
- Console logs show:
  - State changes (Idle → Patrol → Chase → Attack)
  - Direction choices
  - Player detection
  - Fire decisions

## AI Behavior Details

### Patrol Behavior
- Chooses random cardinal direction
- Avoids blocked paths (checks for obstacles)
- Changes direction every 3 seconds
- Fires randomly with low probability (15%)

### Chase Behavior
- Moves toward player position
- Maintains pursuit even through obstacles
- Transitions to Attack when in range
- Returns to Patrol if player lost for 3 seconds

### Attack Behavior
- Faces player direction
- Fires with higher probability (30%)
- Doesn't move (stays in position)
- Returns to Chase if player moves out of range

### Detection System
- Uses detection range to find player (10 units)
- Requires line of sight (no obstacles blocking)
- Raycast-based obstacle detection
- Works with Unity's LayerMask system

## Performance Considerations

### Optimization Features
1. **Max Active Enemies**: Limits concurrent enemies (default: 3)
2. **Spawn Timing**: Controlled intervals prevent spawn flooding
3. **State Machine**: Efficient decision-making
4. **Obstacle Caching**: Minimal raycast usage

### Memory Management
- Enemies removed from active list on death
- Cleanup of null references
- Object destruction (pooling can be added later)

## Extending the System

### Adding New Enemy Types
1. Create new TankData asset in Resources/TankData/
2. Create new prefab in Resources/Prefabs/
3. Add type constant to EnemyTypes.cs
4. Update helper methods in EnemyTypes
5. Add to enemy prefab array in GameSceneController

### Adding New AI States
1. Add state to AIState enum
2. Create UpdateXXXState() method
3. Add state transitions in ChangeState()
4. Implement EnterState() and ExitState() logic

### Custom Behaviors
Override or extend:
- `EnemyController.Move()` for custom movement
- `EnemyAI.UpdateXXXState()` for state behaviors
- `EnemySpawner.DetermineEnemyType()` for spawn logic

## Testing Checklist

- [ ] Enemies spawn at defined intervals
- [ ] Enemies move in cardinal directions
- [ ] AI transitions through states (Idle → Patrol)
- [ ] Enemies detect player (Patrol → Chase)
- [ ] Enemies attack when in range (Chase → Attack)
- [ ] Enemies fire projectiles
- [ ] Health system works correctly
- [ ] Different enemy types have different speeds
- [ ] Collision detection prevents wall clipping
- [ ] Wave progression works correctly

## Troubleshooting

### Enemies Don't Spawn
- Check GameSceneController has EnemySpawner reference
- Verify enemy prefabs loaded (check console logs)
- Ensure Wave1.asset exists in Resources/Waves/
- Check spawn points are defined

### AI Doesn't Work
- Verify PlayerController exists in scene
- Check Layer Mask for obstacle detection
- Enable debugMode in EnemyAI to see state transitions
- Ensure Rigidbody2D settings correct (gravity = 0)

### Enemies Don't Move
- Check TankData has moveSpeed > 0
- Verify Rigidbody2D is not kinematic
- Ensure collision layers don't prevent movement
- Check EnemyController.Initialize() was called

### Performance Issues
- Reduce max active enemies
- Increase spawn interval
- Disable debug logging in production
- Consider object pooling for large enemy counts

## Future Enhancements

Possible improvements:
1. Object pooling for better performance
2. Advanced pathfinding (A* algorithm)
3. Formation movement
4. Boss enemy types
5. Special attack patterns
6. Team AI coordination
7. Difficulty scaling
8. Visual effects for state changes
9. Audio feedback for actions
10. Analytics for AI behavior tuning

## References

- EnemyController.cs - Tank control and physics
- EnemyAI.cs - State machine implementation
- EnemySpawner.cs - Wave management
- EnemyTypes.cs - Type definitions
- GameSceneController.cs - Integration
- EventBus.cs - Event communication
- TankData.cs - Configuration data structure
