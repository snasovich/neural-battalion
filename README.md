# Neural Battalion

A 2D tank combat game inspired by the classic Battle City, built with Unity and C#.

## 🎮 Game Overview

Neural Battalion is a modern take on the classic Battle City tank game. Players control a tank to defend their base while destroying enemy tanks across multiple levels with destructible terrain.

## 📁 Project Structure

```
neural-battalion/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/                    # Core game systems
│   │   │   ├── GameManager.cs       # Main game state management (Singleton)
│   │   │   ├── GameLoop.cs          # Update loop orchestration
│   │   │   ├── LevelManager.cs      # Level loading/transitions
│   │   │   ├── ScoreManager.cs      # Score tracking and persistence
│   │   │   └── Events/              # Event system
│   │   │       ├── GameEvents.cs    # Event definitions
│   │   │       └── EventBus.cs      # Pub/sub event bus
│   │   │
│   │   ├── Player/                  # Player-related scripts
│   │   │   ├── PlayerController.cs  # Main player tank controller
│   │   │   ├── PlayerInput.cs       # Input handling abstraction
│   │   │   ├── PlayerHealth.cs      # Player health/lives management
│   │   │   └── PlayerUpgrades.cs    # Power-up/upgrade system
│   │   │
│   │   ├── Enemy/                   # Enemy AI and management
│   │   │   ├── EnemyController.cs   # Base enemy tank controller
│   │   │   ├── EnemyAI.cs           # AI behavior (State Machine)
│   │   │   ├── EnemySpawner.cs      # Enemy wave spawning logic
│   │   │   └── EnemyTypes.cs        # Enemy type definitions
│   │   │
│   │   ├── Terrain/                 # Terrain and map systems
│   │   │   ├── TerrainManager.cs    # Terrain grid management
│   │   │   ├── TileTypes.cs         # Tile type definitions
│   │   │   ├── DestructibleTerrain.cs # Destructible terrain logic
│   │   │   └── BaseController.cs    # Player base (protect objective)
│   │   │
│   │   ├── Combat/                  # Combat mechanics
│   │   │   ├── Projectile.cs        # Bullet/projectile behavior
│   │   │   ├── DamageSystem.cs      # Damage calculation
│   │   │   ├── Weapon.cs            # Weapon properties
│   │   │   └── Explosion.cs         # Explosion effects
│   │   │
│   │   ├── UI/                      # User interface
│   │   │   ├── HUDController.cs     # In-game HUD
│   │   │   ├── MenuController.cs    # Menu navigation
│   │   │   ├── PauseMenu.cs         # Pause functionality
│   │   │   └── GameOverScreen.cs    # Game over display
│   │   │
│   │   ├── Audio/                   # Audio management
│   │   │   ├── AudioManager.cs      # Sound playback (Singleton)
│   │   │   └── SoundLibrary.cs      # Sound clip references
│   │   │
│   │   ├── Utility/                 # Helper utilities
│   │   │   ├── ObjectPool.cs        # Object pooling for performance
│   │   │   ├── ServiceLocator.cs    # Service locator pattern
│   │   │   └── Constants.cs         # Game constants
│   │   │
│   │   └── Data/                    # Scriptable Objects & Data
│   │       ├── TankData.cs          # Tank configuration SO
│   │       ├── LevelData.cs         # Level configuration SO
│   │       └── WaveData.cs          # Enemy wave configuration SO
│   │
│   ├── Prefabs/                     # Reusable game objects
│   │   ├── Tanks/                   # Tank prefabs
│   │   ├── Projectiles/             # Bullet prefabs
│   │   ├── Terrain/                 # Terrain tile prefabs
│   │   ├── UI/                      # UI element prefabs
│   │   └── Effects/                 # VFX prefabs
│   │
│   ├── Scenes/                      # Unity scenes
│   │   ├── MainMenu.unity
│   │   ├── GameScene.unity
│   │   └── LevelEditor.unity
│   │
│   ├── Art/                         # Visual assets
│   │   ├── Sprites/                 # 2D sprite textures
│   │   ├── Animations/              # Animation clips
│   │   └── UI/                      # UI graphics
│   │
│   ├── Audio/                       # Audio assets
│   │   ├── SFX/                     # Sound effects
│   │   └── Music/                   # Background music
│   │
│   ├── Settings/                    # Unity settings assets
│   │   └── InputActions.inputactions
│   │
│   └── Editor/                      # Editor-only scripts
│       └── LevelEditorWindow.cs     # Custom level editor
│
├── .gitignore
├── LICENSE
└── README.md
```

## 🏗️ Architecture & Design Patterns

### Namespaces

```csharp
NeuralBattalion.Core        // Core game systems
NeuralBattalion.Player      // Player-related functionality
NeuralBattalion.Enemy       // Enemy AI and management
NeuralBattalion.Terrain     // Map and terrain systems
NeuralBattalion.Combat      // Combat mechanics
NeuralBattalion.UI          // User interface
NeuralBattalion.Audio       // Audio management
NeuralBattalion.Utility     // Helper utilities
NeuralBattalion.Data        // Data structures and ScriptableObjects
```

### Design Patterns Used

| Pattern | Usage | Location |
|---------|-------|----------|
| **Singleton** | GameManager, AudioManager | Core, Audio |
| **State Machine** | Enemy AI, Game States | Enemy, Core |
| **Observer/Event Bus** | Game events, UI updates | Core/Events |
| **Object Pool** | Projectiles, Effects | Utility |
| **Service Locator** | System access | Utility |
| **Strategy** | Enemy behaviors | Enemy |
| **Factory** | Tank/Enemy creation | Core |
| **ScriptableObject** | Configuration data | Data |

## 🔄 Game Loop Responsibilities

### GameLoop.cs

The `GameLoop` class orchestrates the main update cycle:

```
┌─────────────────────────────────────────────────────────────┐
│                      GAME LOOP                              │
├─────────────────────────────────────────────────────────────┤
│  1. ProcessInput()      - Handle player input               │
│  2. UpdateAI()          - Update enemy AI decisions         │
│  3. UpdatePhysics()     - Process movement & collisions     │
│  4. UpdateCombat()      - Handle projectiles & damage       │
│  5. UpdateTerrain()     - Handle terrain destruction        │
│  6. CheckWinConditions()- Evaluate game state               │
│  7. UpdateUI()          - Refresh HUD elements              │
└─────────────────────────────────────────────────────────────┘
```

### GameManager States

```
                    ┌──────────────┐
                    │   MainMenu   │
                    └──────┬───────┘
                           │ Start Game
                           ▼
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│    Paused    │◄───│   Playing    │───►│   GameOver   │
└──────┬───────┘    └──────┬───────┘    └──────────────┘
       │                   │
       └───────────────────┘
              Resume
```

## 🎯 System Responsibilities

### Player System

| Component | Responsibility |
|-----------|----------------|
| `PlayerController` | Movement, rotation, shooting commands |
| `PlayerInput` | Abstract input layer (keyboard/gamepad) |
| `PlayerHealth` | Lives, damage taken, respawning |
| `PlayerUpgrades` | Power-ups, weapon upgrades, shield |

### Enemy System

| Component | Responsibility |
|-----------|----------------|
| `EnemyController` | Base movement, shooting behavior |
| `EnemyAI` | Decision making (patrol, chase, attack) |
| `EnemySpawner` | Wave management, spawn timing |
| `EnemyTypes` | Tank variants (light, heavy, fast) |

**AI State Machine:**
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

### Terrain System

| Component | Responsibility |
|-----------|----------------|
| `TerrainManager` | Tile grid, pathfinding data |
| `TileTypes` | Brick, Steel, Water, Trees, Ice |
| `DestructibleTerrain` | Damage handling, destruction |
| `BaseController` | Player base protection, game over trigger |

**Tile Properties:**
| Tile Type | Destructible | Passable | Bullet Pass |
|-----------|--------------|----------|-------------|
| Brick | ✅ | ❌ | ❌ |
| Steel | ❌* | ❌ | ❌ |
| Water | ❌ | ❌ | ✅ |
| Trees | ❌ | ✅ | ✅ |
| Ice | ❌ | ✅ | ✅ |
| Empty | N/A | ✅ | ✅ |

*Steel can be destroyed with power-ups

### Combat System

| Component | Responsibility |
|-----------|----------------|
| `Projectile` | Movement, collision detection |
| `DamageSystem` | Damage calculation, health reduction |
| `Weapon` | Fire rate, projectile speed, damage |
| `Explosion` | Visual/audio feedback, area damage |

## 🚀 Getting Started

### Prerequisites

- Unity 2022.3 LTS or newer
- Visual Studio 2022 or VS Code with C# extension

### Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/snasovich/neural-battalion.git
   ```

2. Open the project in Unity Hub

3. Open `Assets/Scenes/MainMenu.unity`

4. Press Play to test

## 🎮 Controls

| Action | Keyboard | Gamepad |
|--------|----------|---------|
| Move | WASD / Arrow Keys | Left Stick |
| Fire | Space / Enter | A / X Button |
| Pause | Escape | Start |

## 🔄 CI/CD

This project uses GitHub Actions for continuous integration to ensure code quality:

- **Automated Builds**: Every pull request and push to main triggers a Unity build to verify compilation
- **Build Status**: PR checks prevent merging code that breaks compilation
- **Setup Guide**: See [.github/workflows/README.md](.github/workflows/README.md) for CI setup instructions

## 🛠️ Development Roadmap

- [ ] Core game loop implementation
- [ ] Player tank movement and shooting
- [ ] Basic enemy AI
- [ ] Terrain system with destructible blocks
- [ ] Level loading from data
- [ ] Power-up system
- [ ] Sound effects and music
- [ ] Multiple levels
- [ ] High score system

## 📝 Class Reference

### Core Classes

```csharp
// GameManager - Singleton managing game state
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; }
    public GameState CurrentState { get; }
    public void StartGame();
    public void PauseGame();
    public void EndGame(bool victory);
}

// EventBus - Publish/Subscribe event system
public static class EventBus
{
    public static void Publish<T>(T eventData);
    public static void Subscribe<T>(Action<T> handler);
    public static void Unsubscribe<T>(Action<T> handler);
}
```

### Tank Classes

```csharp
// Base tank functionality
public abstract class TankController : MonoBehaviour
{
    public TankData TankData { get; }
    public void Move(Vector2 direction);
    public void Rotate(float angle);
    public void Fire();
    public void TakeDamage(int damage);
}
```

## 📄 License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Inspired by the classic Battle City (1985) by Namco
- Built with Unity Game Engine