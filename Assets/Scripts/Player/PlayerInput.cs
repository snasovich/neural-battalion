using UnityEngine;
using UnityEngine.InputSystem;

namespace NeuralBattalion.Player
{
    /// <summary>
    /// Handles player input abstraction.
    /// Responsibilities:
    /// - Abstract input from specific devices (keyboard, gamepad)
    /// - Provide unified input interface
    /// - Handle input buffering if needed
    /// 
    /// Supports both legacy Input Manager and new Input System.
    /// </summary>
    public class PlayerInput : MonoBehaviour
    {
        [Header("Input Settings")]
        [SerializeField] private bool useNewInputSystem = false;
        [SerializeField] private float inputDeadzone = 0.1f;

        [Header("Legacy Input Keys")]
        [SerializeField] private KeyCode fireKey = KeyCode.Space;
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

        // New Input System references (optional)
        private InputAction moveAction;
        private InputAction fireAction;
        private InputAction pauseAction;

        // Input state
        private Vector2 currentMoveInput;
        private bool firePressed;
        private bool pausePressed;

        private void Awake()
        {
            if (useNewInputSystem)
            {
                SetupNewInputSystem();
            }
        }

        private void OnEnable()
        {
            if (useNewInputSystem)
            {
                EnableNewInputActions();
            }
        }

        private void OnDisable()
        {
            if (useNewInputSystem)
            {
                DisableNewInputActions();
            }
        }

        private void Update()
        {
            if (!useNewInputSystem)
            {
                ReadLegacyInput();
            }
        }

        /// <summary>
        /// Setup the new Input System actions.
        /// </summary>
        private void SetupNewInputSystem()
        {
            // TODO: Load from InputActions asset or create programmatically
            // moveAction = inputActions.FindAction("Move");
            // fireAction = inputActions.FindAction("Fire");
            // pauseAction = inputActions.FindAction("Pause");
        }

        private void EnableNewInputActions()
        {
            moveAction?.Enable();
            fireAction?.Enable();
            pauseAction?.Enable();
        }

        private void DisableNewInputActions()
        {
            moveAction?.Disable();
            fireAction?.Disable();
            pauseAction?.Disable();
        }

        /// <summary>
        /// Read input from legacy Input Manager.
        /// </summary>
        private void ReadLegacyInput()
        {
            // Movement input (WASD or Arrow keys)
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            currentMoveInput = new Vector2(horizontal, vertical);

            // Apply deadzone
            if (currentMoveInput.magnitude < inputDeadzone)
            {
                currentMoveInput = Vector2.zero;
            }

            // Fire input
            firePressed = Input.GetKey(fireKey);

            // Pause input
            pausePressed = Input.GetKeyDown(pauseKey);
        }

        /// <summary>
        /// Get the current movement direction.
        /// </summary>
        /// <returns>Normalized movement direction.</returns>
        public Vector2 GetMoveDirection()
        {
            if (useNewInputSystem && moveAction != null)
            {
                return moveAction.ReadValue<Vector2>();
            }

            // Only normalize if magnitude is significant (avoid normalizing decay values from Input gravity)
            if (currentMoveInput.magnitude > 0.5f)
            {
                return currentMoveInput.normalized;
            }

            return Vector2.zero;
        }

        /// <summary>
        /// Get raw movement input without normalization.
        /// </summary>
        /// <returns>Raw movement input.</returns>
        public Vector2 GetRawMoveInput()
        {
            if (useNewInputSystem && moveAction != null)
            {
                return moveAction.ReadValue<Vector2>();
            }

            return currentMoveInput;
        }

        /// <summary>
        /// Check if fire button is pressed.
        /// </summary>
        /// <returns>True if fire is pressed.</returns>
        public bool IsFirePressed()
        {
            bool isPressed = false;
            
            if (useNewInputSystem && fireAction != null)
            {
                isPressed = fireAction.IsPressed();
            }
            else
            {
                isPressed = firePressed;
            }
            
            if (isPressed)
            {
                Debug.Log($"[PlayerInput] Fire button pressed! useNewInputSystem: {useNewInputSystem}, fireKey: {fireKey}");
            }

            return isPressed;
        }

        /// <summary>
        /// Check if fire button was just pressed this frame.
        /// </summary>
        /// <returns>True if fire was pressed this frame.</returns>
        public bool IsFireDown()
        {
            if (useNewInputSystem && fireAction != null)
            {
                return fireAction.WasPressedThisFrame();
            }

            return Input.GetKeyDown(fireKey);
        }

        /// <summary>
        /// Check if pause button was pressed.
        /// </summary>
        /// <returns>True if pause was pressed this frame.</returns>
        public bool IsPausePressed()
        {
            if (useNewInputSystem && pauseAction != null)
            {
                return pauseAction.WasPressedThisFrame();
            }

            return pausePressed;
        }

        /// <summary>
        /// Get the horizontal input axis.
        /// </summary>
        /// <returns>Horizontal input (-1 to 1).</returns>
        public float GetHorizontal()
        {
            return GetMoveDirection().x;
        }

        /// <summary>
        /// Get the vertical input axis.
        /// </summary>
        /// <returns>Vertical input (-1 to 1).</returns>
        public float GetVertical()
        {
            return GetMoveDirection().y;
        }

        /// <summary>
        /// Check if any movement input is active.
        /// </summary>
        /// <returns>True if there is movement input.</returns>
        public bool HasMoveInput()
        {
            return GetMoveDirection().sqrMagnitude > inputDeadzone * inputDeadzone;
        }
    }
}
