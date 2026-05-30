using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotbarUI : MonoBehaviour
{
    [Header("Slot Root GameObjects (1-5 in order)")]
    public GameObject[] slotRoots;

    [Header("Colors")]
    public Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    public Color selectedColor = new Color(0.9f, 0.7f, 0.1f, 1.0f);
    public Color emptyIconColor = new Color(1, 1, 1, 0.15f);

    [Header("References")]
    public PlayerInventory inventory;

    // ---------------------------------------------------------------
    // Unity lifecycle
    // ---------------------------------------------------------------

    private void Start()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<PlayerInventory>();

        Refresh();
        HighlightSlot(inventory != null ? inventory.SelectedSlot : 0);
    }

    // ---------------------------------------------------------------
    // Called by PlayerInventory events
    // ---------------------------------------------------------------

    public void OnSlotChanged(int newSlot)
    {
        HighlightSlot(newSlot);
    }

    public void Refresh()
    {
        if (inventory == null) return;

        for (int i = 0; i < slotRoots.Length; i++)
        {
            ToolItem item = inventory.GetSlotItem(i);
            SetSlotContent(i, item);
        }
    }

    // ---------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------

    private void HighlightSlot(int selected)
    {
        for (int i = 0; i < slotRoots.Length; i++)
        {
            Image bg = slotRoots[i].GetComponent<Image>();
            if (bg != null)
                bg.color = (i == selected) ? selectedColor : normalColor;
        }
    }

    private void SetSlotContent(int index, ToolItem item)
    {
        if (index >= slotRoots.Length) return;

        // Find icon child
        Transform iconTrans = slotRoots[index].transform.Find("Icon");
        if (iconTrans != null)
        {
            Image iconImg = iconTrans.GetComponent<Image>();
            if (iconImg != null)
            {
                iconImg.sprite = item?.icon;
                iconImg.color = item != null ? Color.white : emptyIconColor;
            }
        }

        // Find label child 
        Transform labelTrans = slotRoots[index].transform.Find("Label");
        if (labelTrans != null)
        {
            TextMeshProUGUI lbl = labelTrans.GetComponent<TextMeshProUGUI>();
            if (lbl != null)
                lbl.text = item != null ? item.toolName : string.Empty;
        }
    }


}
