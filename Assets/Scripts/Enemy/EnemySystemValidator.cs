using UnityEngine;
using NeuralBattalion.Data;

namespace NeuralBattalion.Enemy
{
    /// <summary>
    /// Validation script to verify enemy system is properly configured.
    /// Attach to a GameObject in the scene to run validation checks.
    /// </summary>
    public class EnemySystemValidator : MonoBehaviour
    {
        [Header("Validation Settings")]
        [SerializeField] private bool runOnStart = true;
        [SerializeField] private bool logDetails = true;

        private void Start()
        {
            if (runOnStart)
            {
                ValidateSystem();
            }
        }

        /// <summary>
        /// Run all validation checks for the enemy system.
        /// </summary>
        [ContextMenu("Validate Enemy System")]
        public void ValidateSystem()
        {
            Debug.Log("=== Enemy System Validation Started ===");
            
            bool allPassed = true;
            allPassed &= ValidateTankData();
            allPassed &= ValidatePrefabs();
            allPassed &= ValidateWaveData();
            allPassed &= ValidateScripts();
            
            if (allPassed)
            {
                Debug.Log("✅ <color=green>All validation checks PASSED!</color>");
            }
            else
            {
                Debug.LogWarning("⚠️ <color=yellow>Some validation checks FAILED - see details above</color>");
            }
            
            Debug.Log("=== Enemy System Validation Complete ===");
        }

        private bool ValidateTankData()
        {
            Debug.Log("--- Validating TankData Assets ---");
            bool passed = true;
            
            string[] tankDataPaths = {
                "TankData/EnemyBasic",
                "TankData/EnemyFast",
                "TankData/EnemyPower",
                "TankData/EnemyArmor"
            };
            
            foreach (string path in tankDataPaths)
            {
                TankData data = Resources.Load<TankData>(path);
                if (data == null)
                {
                    Debug.LogError($"❌ Failed to load {path}");
                    passed = false;
                }
                else if (logDetails)
                {
                    Debug.Log($"✓ {path}: {data.TankName} - Speed: {data.MoveSpeed}, Health: {data.Health}");
                }
            }
            
            return passed;
        }

        private bool ValidatePrefabs()
        {
            Debug.Log("--- Validating Enemy Prefabs ---");
            bool passed = true;
            
            string[] prefabPaths = {
                "Prefabs/EnemyBasicTank",
                "Prefabs/EnemyFastTank",
                "Prefabs/EnemyPowerTank",
                "Prefabs/EnemyArmorTank"
            };
            
            foreach (string path in prefabPaths)
            {
                GameObject prefab = Resources.Load<GameObject>(path);
                if (prefab == null)
                {
                    Debug.LogError($"❌ Failed to load {path}");
                    passed = false;
                    continue;
                }
                
                // Check for required components
                var controller = prefab.GetComponent<EnemyController>();
                var ai = prefab.GetComponent<EnemyAI>();
                var rb = prefab.GetComponent<Rigidbody2D>();
                
                if (controller == null)
                {
                    Debug.LogError($"❌ {path} missing EnemyController component");
                    passed = false;
                }
                if (ai == null)
                {
                    Debug.LogError($"❌ {path} missing EnemyAI component");
                    passed = false;
                }
                if (rb == null)
                {
                    Debug.LogError($"❌ {path} missing Rigidbody2D component");
                    passed = false;
                }
                
                if (controller != null && ai != null && rb != null && logDetails)
                {
                    Debug.Log($"✓ {path}: All required components present");
                }
            }
            
            return passed;
        }

        private bool ValidateWaveData()
        {
            Debug.Log("--- Validating Wave Data ---");
            bool passed = true;
            
            WaveData wave = Resources.Load<WaveData>("Waves/Wave1");
            if (wave == null)
            {
                Debug.LogError("❌ Failed to load Waves/Wave1");
                return false;
            }
            
            if (!wave.IsValid())
            {
                Debug.LogError("❌ Wave1 data is invalid");
                passed = false;
            }
            else if (logDetails)
            {
                Debug.Log($"✓ Wave1: {wave.TotalEnemies} enemies, " +
                         $"Spawn interval: {wave.SpawnInterval}s, " +
                         $"Max active: {wave.MaxActiveEnemies}");
            }
            
            return passed;
        }

        private bool ValidateScripts()
        {
            Debug.Log("--- Validating Enemy Scripts ---");
            bool passed = true;
            
            // Check if AI states are properly defined
            var aiStates = System.Enum.GetValues(typeof(AIState));
            if (aiStates.Length != 4)
            {
                Debug.LogError($"❌ Expected 4 AI states, found {aiStates.Length}");
                passed = false;
            }
            else if (logDetails)
            {
                Debug.Log($"✓ AI States: {string.Join(", ", System.Enum.GetNames(typeof(AIState)))}");
            }
            
            // Check enemy types
            if (EnemyTypes.Count != 4)
            {
                Debug.LogError($"❌ Expected 4 enemy types, found {EnemyTypes.Count}");
                passed = false;
            }
            else if (logDetails)
            {
                Debug.Log($"✓ Enemy Types: Basic, Fast, Power, Armor");
            }
            
            return passed;
        }

        /// <summary>
        /// Test enemy movement by simulating different directions.
        /// </summary>
        [ContextMenu("Test Enemy Movement")]
        public void TestEnemyMovement()
        {
            Debug.Log("=== Testing Enemy Movement ===");
            
            // Spawn a test enemy
            GameObject prefab = Resources.Load<GameObject>("Prefabs/EnemyBasicTank");
            if (prefab == null)
            {
                Debug.LogError("❌ Cannot test movement - prefab not found");
                return;
            }
            
            GameObject testEnemy = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            testEnemy.name = "TestEnemy";
            
            EnemyController controller = testEnemy.GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.Initialize(999, 0);
                Debug.Log("✓ Test enemy spawned and initialized");
                
                // Test movement in different directions
                Debug.Log("Testing movement directions...");
                controller.SetMoveDirection(Vector2.up);
                Debug.Log("- Set direction: UP");
                controller.SetMoveDirection(Vector2.right);
                Debug.Log("- Set direction: RIGHT");
                
                Debug.Log("✅ Movement test complete - enemy should move when physics updates");
            }
            else
            {
                Debug.LogError("❌ Test enemy missing EnemyController");
            }
        }
    }
}
