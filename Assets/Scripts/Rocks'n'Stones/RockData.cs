using UnityEngine;

[CreateAssetMenu(fileName = "RockData", menuName = "Resources/RockData")]
public class RockData : ScriptableObject
{
    [Header("Stage Meshes (1 = full, 4 = last stage before destruction")]
    public GameObject stage1;
    public GameObject stage2;
    public GameObject stage3;
    public GameObject stage4;

    [Header("Spawn")]
    public GameObject rockItemPrefab;
    public int rocksToSpawn = 3;
}
