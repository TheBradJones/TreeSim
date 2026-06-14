using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public PlayerMovement player;
    private InputAction moveAction, lookAction, jumpAction, dropAction;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Start()
    {
        // Get Input Actions
        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        jumpAction = InputSystem.actions.FindAction("Jump");

        jumpAction.performed += OnJumpPerformed;

    }

    void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
            Cursor.lockState = CursorLockMode.Locked;

        // Read values from input actions
        Vector2 movementVector = moveAction.ReadValue<Vector2>();
        player.Move(movementVector);

        Vector2 lookVector = lookAction.ReadValue<Vector2>();
        player.Rotate(lookVector);

    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        player.Jump();  // Call Jump Function within PlayerMovementScript
    }
}
