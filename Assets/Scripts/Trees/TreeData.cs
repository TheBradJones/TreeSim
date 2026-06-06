using UnityEngine;

[CreateAssetMenu(fileName = "NewTreeData", menuName = "Trees/TreeData")]
public class TreeData : ScriptableObject
{
    public TreeType treeType;
    public GameObject upperPrefab;
    public GameObject stumpPrefab;
    public GameObject logPrefab;
}
