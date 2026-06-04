using UnityEngine;

public enum TreeType
{
    Oak,
    Pine,
    Birch,
    DarkOak,
    Cherry
}

[CreateAssetMenu(fileName = "NewTreeData", menuName = "Trees/TreeData")]
public class TreeData : ScriptableObject
{

    public TreeType treeType;           // Type of tree
    public GameObject logPrefab;        // Spawns after tree fallen
    public GameObject stump;            // Always stays
    public GameObject upper;            // Detach on fall
}
