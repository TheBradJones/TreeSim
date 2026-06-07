using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;
    private Camera playerCamera;

    public float MovementSpeed = 10f, RotationSpeed = 5f, jumpForce = 10f, gravity = -30f;

    private float rotX;
    private float rotY;
    private float verticalVelocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterController = GetComponent<CharacterController>();

        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    public void Move(Vector2 movementVector)
    {
        // Basic WASD Movement
        Vector3 camForward = playerCamera.transform.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = playerCamera.transform.right;
        camRight.y = 0;
        camRight.Normalize();

        Vector3 move = camForward * movementVector.y + camRight * movementVector.x;
        characterController.Move(move * MovementSpeed * Time.deltaTime);

        if (characterController.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        // Basic Jump Movement
        verticalVelocity += gravity * Time.deltaTime;
        characterController.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
    }

    public void Rotate(Vector2 rotationVector)
    {
        // Basic X axis rotation
        rotY += rotationVector.x * RotationSpeed * Time.deltaTime;
        rotX -= rotationVector.y * RotationSpeed * Time.deltaTime;

        Vector2 rotation = new Vector2(Mathf.Clamp(rotX, -85f, 85f), rotY);

        playerCamera.transform.localRotation = Quaternion.Euler(rotation.x, rotation.y, 0);
    }

    public void Jump()
    {
        if (characterController.isGrounded)
        {
            verticalVelocity = jumpForce;
        }
    }

}
