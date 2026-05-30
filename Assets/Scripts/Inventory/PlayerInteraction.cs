using UnityEngine;

[RequireComponent(typeof(PlayerInventory))]
public class PlayerInteraction : MonoBehaviour
{
    [Header("Raycast")]
    public float interactRange = 3f;
    public LayerMask interactLayers = ~0;   // Everything by default; restrict as needed

    [Header("Camera")]
    public Camera playerCamera;

    [Header("Key Bindings")]
    public KeyCode interactKey = KeyCode.E;
    public KeyCode returnKey   = KeyCode.R;

    // ---------------------------------------------------------------
    // Runtime
    // ---------------------------------------------------------------

    private PlayerInventory inventory;
    private StorageSlot lookedAt;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    private void Update()
    {
        UpdateLookTarget();
        HandleInput();
    }

    // ---------------------------------------------------------------
    // Look target
    // ---------------------------------------------------------------
    
    private void UpdateLookTarget()
    {
        lookedAt = null;

        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayers))
            lookedAt = hit.collider.GetComponentInParent<StorageSlot>();
    }

    // ---------------------------------------------------------------
    // Input
    // ---------------------------------------------------------------

    private void HandleInput()
    {
        if (Input.GetKeyDown(interactKey) && lookedAt != null)
            lookedAt.OnPlayerInteract(inventory);
        if (Input.GetKeyDown(returnKey))
            inventory.ReturnHeldToStorage();
    }


    // ---------------------------------------------------------------
    // Gizmo (editor only)
    // ---------------------------------------------------------------

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (playerCamera == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactRange);
    }
#endif
}
