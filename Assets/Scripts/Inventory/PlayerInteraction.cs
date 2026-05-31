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
    private LogCarrySystem logCarry;

    private StorageSlot lookedAtStorage;
    private WorldLog lookedAtLog;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        logCarry = GetComponent<LogCarrySystem>();

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

        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayers)) return;

        // Check for a storage slot (tool rack)
        lookedAtStorage = hit.collider.GetComponentInParent<StorageSlot>();

        // Check for a world log (only if no storage slot found)
        if (lookedAtStorage == null)
            lookedAtLog = hit.collider.GetComponentInParent<WorldLog>();
    }

    // ---------------------------------------------------------------
    // Input
    // ---------------------------------------------------------------

    private void HandleInput()
    {
        if (Input.GetKeyDown(interactKey))
        {
            // Priority 1: pickup a log
            if (lookedAtLog != null && lookedAtLog.IsPickupable && logCarry != null)
            {
                logCarry.TryPickupLog(lookedAtLog);
                return;
            }

            // Priority 2: interact with a tool storage slot
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
                    if (logCarry != null && logCarry.IsCarrying)
                    {
                        Debug.Log("[PlayerInteraction] Put down your logs before returning tools.");
                        return;
                    }

                    inventory.ReturnHeldToStorage();
                    return;
                }
                    
                // Storage slot has an item - pick it up
                if (logCarry != null && logCarry.IsCarrying)
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
