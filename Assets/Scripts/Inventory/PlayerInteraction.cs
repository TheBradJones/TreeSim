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

    // ---------------------------------------------------------------
    // Runtime
    // ---------------------------------------------------------------

    private PlayerInventory inventory;
    private ResourceCarrySystem carrySystem;

    private StorageSlot lookedAtStorage;
    private WorldLog lookedAtLog;
    private WorldRock lookedAtRock;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        carrySystem = GetComponent<ResourceCarrySystem>();

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
        lookedAtStorage = null;
        lookedAtLog = null;
        lookedAtRock = null;

        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayers)) return;

        lookedAtStorage = hit.collider.GetComponentInParent<StorageSlot>();

        // Check for a storage slot (tool rack)
        if (lookedAtStorage == null)
            lookedAtLog = hit.collider.GetComponentInParent<WorldLog>();

        // Check for a world log (only if no storage slot found)
        if (lookedAtStorage == null)
            lookedAtRock = hit.collider.GetComponentInParent<WorldRock>();
    }

    // ---------------------------------------------------------------
    // Input
    // ---------------------------------------------------------------

    private void HandleInput()
    {
        if (Input.GetKeyDown(interactKey))
        {
            // Priority 1: pickup a log
            if (lookedAtLog != null && lookedAtLog.IsPickupable && carrySystem != null)
            {
                carrySystem.TryPickupLog(lookedAtLog);
                return;
            }

            // Priority 2: pickup a log
            if (lookedAtRock != null && lookedAtRock.IsPickupable && carrySystem != null)
            {
                carrySystem.TryPickupRock(lookedAtRock);
                return;
            }

            // Priority 3: interact with a tool storage slot
            if (lookedAtStorage != null)
            {
                if (!lookedAtStorage.HasItem)
                {
                    ToolItem heldTool = inventory.HeldItem;

                    // No tool in hand 
                    if (heldTool == null)
                    {
                        Debug.Log("[PlayerInteraction] Not holding anything to return");
                        return;
                    }

                    // Wrong slot
                    if (heldTool != lookedAtStorage.tool)
                    {
                        Debug.Log($"[PlayerInteraction] '{heldTool.toolName}' does not belong in this slot.");
                        return;
                    }

                    // Block while carrying logs
                    if (carrySystem != null && carrySystem.IsCarrying)
                    {
                        Debug.Log("[PlayerInteraction] Put down your logs before returning tools.");
                        return;
                    }

                    inventory.ReturnHeldToStorage();
                    return;
                }
                    
                // Storage slot has an item - pick it up
                if (carrySystem != null && carrySystem.IsCarrying)
                {
                    Debug.Log("[PlayerInteraction] Put down your logs before Picking up tools.");
                    return;
                }

                lookedAtStorage.OnPlayerInteract(inventory);

                
            }
        }
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
