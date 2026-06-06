using UnityEngine;

[CreateAssetMenu(fileName = "TreeDatabase", menuName = "Trees/TreeDatabase")]
public class TreeTypeDB : ScriptableObject
{
    public TreeData[] treeTypes;

    public TreeData GetData(TreeType type)
    {
        foreach (var data in treeTypes)
            if (data.treeType == type)
                return data;

        Debug.LogWarning($"[TreeTypeDB] No data found for type: {type}");
        return null;
    }
}
