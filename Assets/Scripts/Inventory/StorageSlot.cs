using UnityEngine;
using UnityEngine.InputSystem;

public class StorageSlot : MonoBehaviour
{
    [Header("Config")]
    public ToolItem tool;               // Which tool lives here
    public Transform snapPoint;         // Where the world model sits when stored

    private GameObject worldInstance;   // The visual sitting in the slot
    private bool hasItem = true;        // Starts socketed

    private void Start()
    {
        SpawnWorldModel();
    }

    // ---------------------------------------------------------------
    //                          Public API
    // ---------------------------------------------------------------

    // Returns true if the tool is currently in storage
    public bool HasItem => hasItem; 

    // Called by playerInventory to take the tool out of storage. Returns ToolItem, removes world visual
    public ToolItem TakeItem()  
    {
        if (!hasItem) return null;

        hasItem = false;
        if (worldInstance != null)
            worldInstance.SetActive(false);

        return tool;
    }

    // Called by PlayerInventory to return the tool to storage
    public void ReturnItem(ToolItem returned)
    {
        if (returned != tool)
        {
            Debug.LogWarning($"[StorageSlot] Tried to return '{returned?.toolName}' to wrong slot (expects '{tool?.toolName}').");
            return;
        }

        hasItem = true;
        if (worldInstance != null)
            worldInstance.SetActive(true);
        else
            SpawnWorldModel();
    }

    // ---------------------------------------------------------------
    //                          Private Helpers
    // ---------------------------------------------------------------

    private void SpawnWorldModel()
    {
        if (tool == null || tool.prefab == null) return;

        Transform parent = snapPoint != null ? snapPoint : transform;
        worldInstance = Instantiate(tool.prefab, parent.position, parent.rotation, parent);
    }


    // ---------------------------------------------------------------
    //                        Interaction Trigger 
    // ---------------------------------------------------------------

    // Called by the players interaction system (see PlayerInteraction.cs)
    public void OnPlayerInteract(PlayerInventory inventory)
    {
        if (hasItem)
        {
            inventory.PickUpFromStorage(this);
        }
        else
        {
            Debug.Log($"[StorageSlot] '{tool?.toolName}' slot is empty.");
        }
    }


}

