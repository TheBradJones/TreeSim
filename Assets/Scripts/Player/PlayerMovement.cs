using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;

    public float MovementSpeed = 10f, RotationSpeed = 5f, jumpForce = 10f, gravity = -30f;

    private float rotX;
    private float rotY;
    private float verticalVelocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    public void Move(Vector2 movementVector)
    {
        // Basic WASD Movement
        Vector3 move = transform.forward * movementVector.y + transform.right * movementVector.x;
        move = move * MovementSpeed * Time.deltaTime;
        characterController.Move(move);

        // Basic Jump Movement
        verticalVelocity = verticalVelocity + gravity * Time.deltaTime;
        characterController.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
    }

    public void Rotate(Vector2 rotationVector)
    {
        // Basic X axis rotation
        rotY += rotationVector.x * RotationSpeed * Time.deltaTime;
        rotX -= rotationVector.y * RotationSpeed * Time.deltaTime;

        Vector2 rotation = new Vector2(Mathf.Clamp(rotX, -85, 85), rotY);   // Clamps up/down rotation

        transform.localRotation = Quaternion.Euler(rotation.x, rotation.y, 0);
    }

    public void Jump()
    {
        if (characterController.isGrounded)
        {
            verticalVelocity = jumpForce;
        }
    }
}
