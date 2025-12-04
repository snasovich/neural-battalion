using UnityEngine;
using NeuralBattalion.Player;

namespace NeuralBattalion.Enemy
{
    /// <summary>
    /// AI behavior controller for enemy tanks using State Machine pattern.
    /// Responsibilities:
    /// - Decision making (patrol, chase, attack)
    /// - State transitions
    /// - Target detection
    /// - Navigation
    /// 
    /// AI States:
    /// - IDLE: Waiting at spawn
    /// - PATROL: Random movement pattern
    /// - CHASE: Moving toward player
    /// - ATTACK: Firing at player
    /// </summary>
    public class EnemyAI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemyController controller;
        [SerializeField] private Transform player;

        [Header("Detection Settings")]
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float attackRange = 8f;
        [SerializeField] private LayerMask obstacleLayer;

        [Header("Patrol Settings")]
        [SerializeField] private float patrolIdleTime = 2f;
        [SerializeField] private float directionChangeTime = 3f;

        [Header("Attack Settings")]
        [SerializeField] private float fireChance = 0.3f;
        [SerializeField] private float fireCheckInterval = 0.5f;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

        // State machine
        public AIState CurrentState { get; private set; } = AIState.Idle;

        // State timers
        private float stateTimer;
        private float fireCheckTimer;
        private float idleTimer;

        // Movement
        private Vector2 currentDirection;
        private int consecutiveBlocks;

        private void Awake()
        {
            if (controller == null)
            {
                controller = GetComponent<EnemyController>();
            }
        }

        private void Start()
        {
            // Find player if not assigned
            if (player == null)
            {
                var playerObj = FindObjectOfType<PlayerController>();
                if (playerObj != null)
                {
                    player = playerObj.transform;
                    if (debugMode)
                    {
                        Debug.Log($"[EnemyAI {gameObject.name}] Found player at {player.position}");
                    }
                }
                else if (debugMode)
                {
                    Debug.LogWarning($"[EnemyAI {gameObject.name}] No player found in scene");
                }
            }

            // Start in idle state
            ChangeState(AIState.Idle);
            
            if (debugMode)
            {
                Debug.Log($"[EnemyAI {gameObject.name}] AI initialized - Detection range: {detectionRange}, Attack range: {attackRange}");
            }
        }

        private void Update()
        {
            if (controller == null || !controller.IsAlive || controller.IsFrozen)
            {
                return;
            }

            // Update current state
            switch (CurrentState)
            {
                case AIState.Idle:
                    UpdateIdleState();
                    break;
                case AIState.Patrol:
                    UpdatePatrolState();
                    break;
                case AIState.Chase:
                    UpdateChaseState();
                    break;
                case AIState.Attack:
                    UpdateAttackState();
                    break;
            }

            // Update fire check
            UpdateFireCheck();
        }

        /// <summary>
        /// Change to a new AI state.
        /// </summary>
        /// <param name="newState">New state to transition to.</param>
        private void ChangeState(AIState newState)
        {
            if (debugMode)
            {
                Debug.Log($"[EnemyAI {gameObject.name}] State change: {CurrentState} -> {newState}");
            }

            // Exit current state
            ExitState(CurrentState);

            // Enter new state
            CurrentState = newState;
            stateTimer = 0f;
            EnterState(newState);
        }

        private void EnterState(AIState state)
        {
            switch (state)
            {
                case AIState.Idle:
                    controller.SetMoveDirection(Vector2.zero);
                    idleTimer = patrolIdleTime;
                    break;

                case AIState.Patrol:
                    ChooseRandomDirection();
                    break;

                case AIState.Chase:
                    // Will update direction in Update
                    break;

                case AIState.Attack:
                    controller.SetMoveDirection(Vector2.zero);
                    break;
            }
        }

        private void ExitState(AIState state)
        {
            // Clean up state-specific behavior
        }

        #region State Updates

        private void UpdateIdleState()
        {
            idleTimer -= Time.deltaTime;

            if (idleTimer <= 0)
            {
                ChangeState(AIState.Patrol);
            }

            // Check for player detection
            if (CanSeePlayer())
            {
                ChangeState(AIState.Chase);
            }
        }

        private void UpdatePatrolState()
        {
            stateTimer += Time.deltaTime;

            // Change direction periodically
            if (stateTimer >= directionChangeTime)
            {
                ChooseRandomDirection();
                stateTimer = 0f;
            }

            // Move in current direction
            controller.SetMoveDirection(currentDirection);

            // Check for obstacles (change direction if blocked)
            if (IsBlocked())
            {
                consecutiveBlocks++;
                if (consecutiveBlocks > 3)
                {
                    ChooseRandomDirection();
                    consecutiveBlocks = 0;
                }
            }
            else
            {
                consecutiveBlocks = 0;
            }

            // Check for player detection
            if (CanSeePlayer())
            {
                ChangeState(AIState.Chase);
            }
        }

        private void UpdateChaseState()
        {
            if (player == null)
            {
                ChangeState(AIState.Patrol);
                return;
            }

            // Move toward player
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            controller.SetMoveDirection(directionToPlayer);

            // Check if in attack range
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange && HasLineOfSight())
            {
                ChangeState(AIState.Attack);
            }

            // Lost sight of player
            if (!CanSeePlayer())
            {
                stateTimer += Time.deltaTime;
                if (stateTimer > 3f)
                {
                    ChangeState(AIState.Patrol);
                }
            }
            else
            {
                stateTimer = 0f;
            }
        }

        private void UpdateAttackState()
        {
            if (player == null)
            {
                ChangeState(AIState.Patrol);
                return;
            }

            // Face the player
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            controller.SetMoveDirection(directionToPlayer);

            // Check if player moved out of range
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer > attackRange || !HasLineOfSight())
            {
                ChangeState(AIState.Chase);
            }
        }

        #endregion

        #region Detection & Navigation

        /// <summary>
        /// Check if the player is within detection range and visible.
        /// </summary>
        private bool CanSeePlayer()
        {
            if (player == null) return false;

            float distance = Vector2.Distance(transform.position, player.position);
            if (distance > detectionRange) return false;

            return HasLineOfSight();
        }

        /// <summary>
        /// Check if there's a clear line of sight to the player.
        /// </summary>
        private bool HasLineOfSight()
        {
            if (player == null) return false;

            Vector2 direction = player.position - transform.position;
            float distance = direction.magnitude;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, distance, obstacleLayer);

            return hit.collider == null;
        }

        /// <summary>
        /// Check if movement in current direction is blocked.
        /// </summary>
        private bool IsBlocked()
        {
            if (currentDirection == Vector2.zero) return false;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, currentDirection, 1f, obstacleLayer);
            return hit.collider != null;
        }

        /// <summary>
        /// Choose a random patrol direction.
        /// </summary>
        private void ChooseRandomDirection()
        {
            Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
            currentDirection = directions[Random.Range(0, directions.Length)];

            // Avoid choosing blocked directions
            int attempts = 0;
            while (IsBlocked() && attempts < 4)
            {
                currentDirection = directions[Random.Range(0, directions.Length)];
                attempts++;
            }
            
            if (debugMode)
            {
                Debug.Log($"[EnemyAI {gameObject.name}] Chose direction: {currentDirection} (attempts: {attempts})");
            }
        }

        #endregion

        #region Combat

        /// <summary>
        /// Periodic check for firing opportunity.
        /// </summary>
        private void UpdateFireCheck()
        {
            fireCheckTimer += Time.deltaTime;

            if (fireCheckTimer < fireCheckInterval) return;

            fireCheckTimer = 0f;

            // Fire randomly during patrol
            if (CurrentState == AIState.Patrol)
            {
                if (Random.value < fireChance * 0.5f)
                {
                    controller.TryFire();
                }
            }
            // Fire more often during chase/attack
            else if (CurrentState == AIState.Chase || CurrentState == AIState.Attack)
            {
                if (Random.value < fireChance)
                {
                    controller.TryFire();
                }
            }
        }

        #endregion

        /// <summary>
        /// Set the target player transform.
        /// </summary>
        /// <param name="playerTransform">Player transform to target.</param>
        public void SetPlayer(Transform playerTransform)
        {
            player = playerTransform;
        }

        /// <summary>
        /// Force a state change (for external control).
        /// </summary>
        /// <param name="state">State to change to.</param>
        public void ForceState(AIState state)
        {
            ChangeState(state);
        }
    }

    /// <summary>
    /// Possible AI states.
    /// </summary>
    public enum AIState
    {
        Idle,
        Patrol,
        Chase,
        Attack
    }
}
