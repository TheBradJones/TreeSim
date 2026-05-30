using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    // ---------------------------------------------------
    //                     Inspector
    // ---------------------------------------------------

    [Header("Toolbelt")]
    [Range(1, 5)] public int slotCount = 5;

    [Header("Hand / Equip Point")]
    public Transform handPoint;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent<int> onSlotChanged;
    public UnityEngine.Events.UnityEvent onInventoryChanged;

    // ---------------------------------------------------
    //                   Runtime State
    // ---------------------------------------------------

    private ToolItem[] slots;
    private StorageSlot[] slotSources;
    private GameObject[] handModels;

    private int selectedSlot = 0;
    private float scrollAccum = 0f;

    // ---------------------------------------------------
    //                  Unity Lifecycle
    // ---------------------------------------------------

    private void Awake()
    {
        slots = new ToolItem[slotCount];
        slotSources = new StorageSlot[slotCount];
        handModels = new GameObject[slotCount];
    }

    private void Update()
    {
        HandleKeyboardInput();
        HandleScrollInput();
    }

    // ---------------------------------------------------
    //                      Input
    // ---------------------------------------------------

    private void HandleKeyboardInput()
    {
        for (int i = 0; i < slotCount; i++)
        {
            // Alpha keys: KeyCode.Alpha1 == 49, sequential from there
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
                return;
            }
        }
    }

    private void HandleScrollInput()
    {
        scrollAccum += Input.mouseScrollDelta.y;

        while (scrollAccum >= 1f)
        {
            scrollAccum -= 1f;
            SelectSlot((selectedSlot - 1 + slotCount) % slotCount); // Scroll up -> previous
        }
        while (scrollAccum <= -1f)
        {
            scrollAccum += 1f;
            SelectSlot((selectedSlot + 1) % slotCount);           // Scroll down -> next
        }
    }

    // ---------------------------------------------------------------
    //                          Slot selection
    // ---------------------------------------------------------------

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= slotCount) return;
        if (index == selectedSlot) return;

        SetHandModelVisible(selectedSlot, false);
        selectedSlot = index;
        SetHandModelVisible(selectedSlot, true);

        onSlotChanged?.Invoke(selectedSlot);
    }

    public int SelectedSlot => selectedSlot;
    public ToolItem GetSlotItem(int index) => (index >= 0 && index < slotCount) ? slots[index] : null;
    public ToolItem HeldItem => slots[selectedSlot];

    // ---------------------------------------------------------------
    //                      Pick up from storage
    // ---------------------------------------------------------------

    public bool PickUpFromStorage(StorageSlot storage)
    {
        if (storage == null || !storage.HasItem) return false;

        int targetSlot = FindBestSlot(storage.tool);
        if (targetSlot == -1)
        {
            Debug.Log("[PlayerInventory] Hotbar is full - return a tool to storage first.");
            return false;
        }

        if (slots[targetSlot] != null)
            ReturnToStorage(targetSlot);

        ToolItem item = storage.TakeItem();
        slots[targetSlot]       = item;
        slotSources[targetSlot] = storage;

        SpawnHandModel(targetSlot, item);

        // Switch to the picked up slot automatically
        SelectSlot(targetSlot);

        onInventoryChanged?.Invoke();
        Debug.Log($"[PlayerInventory] Picked up '{item.toolName}' into slot {targetSlot + 1}.");
        return true;
    }

    // ---------------------------------------------------------------
    // Return to storage (no dropping)
    // ---------------------------------------------------------------

    // Returns item in given hotbar slot to its designated storage
    public void ReturnToStorage(int slotIndex)
    {
        if (slots[slotIndex] == null) return;

        ToolItem item   = slots[slotIndex];
        StorageSlot src = slotSources[slotIndex];

        if (src != null)
            src.ReturnItem(item);
        else
            Debug.LogWarning($"[PlayerInventory] No source StorageSlot recorded for '{item.toolName}' - cannot return");

        DestroyHandModel(slotIndex);
        slots[slotIndex]       = null;
        slotSources[slotIndex] = null;

        onInventoryChanged?.Invoke();
        Debug.Log($"[PlayerInventory] Returned '{item.toolName}' to storage from slot {slotIndex + 1}. ");
    }

    public void ReturnHeldToStorage() => ReturnToStorage(selectedSlot);

    // ---------------------------------------------------------------
    //                      Hand model helpers
    // ---------------------------------------------------------------

    private void SpawnHandModel(int slot, ToolItem item)
    {
        DestroyHandModel(slot);
        if (item.prefab == null || handPoint == null) return;

        handModels[slot] = Instantiate(item.prefab, handPoint.position, handPoint.rotation, handPoint);
        handModels[slot].SetActive(slot == selectedSlot);
    }

    private void DestroyHandModel(int slot)
    {
        if (handModels[slot] != null)
        {
            Destroy(handModels[slot]);
            handModels[slot] = null;
        }
    }

    private void SetHandModelVisible(int slot, bool visible)
    {
        if (handModels[slot] != null)
            handModels[slot].SetActive(visible);
    }

    // ---------------------------------------------------------------
    //                      Slot selection helpers
    // ---------------------------------------------------------------

    private int FindBestSlot(ToolItem item)
    {
        /*  This code made it so each item has a specific slot regardless of in which order it was picked up
        
        // Prefer the tools designated hotbar position
        int preferred = item.storageSlotIndex;
        if (preferred >= 0 && preferred < slotCount && slots[preferred] == null)
            return preferred;

        // Fall back to selected slot
        if (slots[selectedSlot] == null)
            return selectedSlot;

        // Fall back to first empty
        for (int i = 0; i < slotCount; i++)
            if (slots[i] == null) return i;

        */

        for (int i = 0; i < slotCount; i++)
            if (slots[i] == null) return i;

        return -1;
    }


}
