using UnityEngine;

[CreateAssetMenu(fileName = "NewTool", menuName = "Inventory/ToolItem")]
public class ToolItem : ScriptableObject
{
    [Header("Identity")]
    public string toolName = "Unnamed Tool";
    public Sprite icon;
    public GameObject prefab;

    [Header("Storage")]
    [Tooltip("Which storage slot index (0-based) this tool belongs to.")]
    public int storageSlotIndex = 0;
}
