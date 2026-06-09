using UnityEngine;
using System.Collections.Generic;

public enum CarryType { None, Logs, Rocks }

[RequireComponent(typeof(PlayerInventory))]
public class ResourceCarrySystem : MonoBehaviour
{
    // ---------------------------------------------------------------
    //                          Inspector
    // ---------------------------------------------------------------

    [Header("Carry Settings")]
    public int maxLogs = 3;
    public int maxRocks = 5;

    [Header("Shoulder Mount")]
    public Transform shoulderPoint; // Logs are parented here visually
    public Vector3 logStackOffset = new Vector3(0, 1, 0);   // Local space offset applied per extra log so they dont all overlap on the shoulder
    public Vector3 rockStackOffset = new Vector3(0, 1000, 0);   // Local space offset applied per extra Stone so they dont all overlap on the shoulder

    [Header("Drop Settings")]
    public KeyCode dropOneKey = KeyCode.G;
    public float holdDropTime = 0.5f;   // Seconds to hold G before dropping all

    [Header("Prefabs")]
    public GameObject logPrefab;    // Prefab spawned in the world a log is dropped. must have a rb and worldlog component
    public GameObject rockPrefab;

    // ---------------------------------------------------------------
    //                          Runtime
    // ---------------------------------------------------------------

    private PlayerInventory inventory;

    private readonly List<GameObject> shoulderModels = new List<GameObject>();
    private readonly List<TreeData> carriedLogData = new List<TreeData>();
    private readonly List<RockData> carriedRockData = new List<RockData>();

    public CarryType carryType = CarryType.None;
    public int resourceCount = 0;

    private float holdTimer = 0f;
    private bool holdFired = false;

    public int ResourceCount => resourceCount;
    public bool IsCarrying => resourceCount > 0;
    public bool CanCarryMore => resourceCount < (carryType == CarryType.Rocks ? maxRocks : maxLogs);

    // ---------------------------------------------------------------
    //                      Unity Lifecycle
    // ---------------------------------------------------------------

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
    }

    private void Update()
    {
        HandleDropInput();
    }

    // ---------------------------------------------------------------
    //                      Pickup Resources
    // ---------------------------------------------------------------

    public bool TryPickupLog(WorldLog worldLog)
    {
        if (worldLog == null) return false;

        if (carryType == CarryType.Rocks)
        {
            Debug.Log("[ResourceCarrySystem] Put down your rocks before picking up logs.");
            return false;
        }

        if (carryType == CarryType.None)
            carryType = CarryType.Logs;

        if (resourceCount >= maxLogs)
        {
            Debug.Log("[ResourceCarrySystem] Already carrying max logs");
            return false;
        }

        TreeData data = worldLog.treeData;
        worldLog.OnPickedUp();

        resourceCount++;
        carriedLogData.Add(data);
        AddShoulderModel(data != null ? data.logPrefab : logPrefab, logStackOffset);

        inventory.SetHandModelsVisible(false);
        return true;
    }

    public bool TryPickupRock(WorldRock worldRock)
    {
        if (worldRock == null) return false;

        if (carryType == CarryType.Logs)
        {
            Debug.Log("[ResourceCarrySystem] Put down your logs before picking up rocks.");
            return false;
        }

        if (carryType == CarryType.None)
            carryType = CarryType.Rocks;

        if (resourceCount >= maxRocks)
        {
            Debug.Log("[ResourceCarrySystem] Already carrying max rocks");
            return false;
        }

        RockData data = worldRock.rockData;
        worldRock.OnPickedUp();

        resourceCount++;
        carriedRockData.Add(data);
        AddShoulderModel(data != null ? data.rockItemPrefab : rockPrefab, rockStackOffset);

        inventory.SetHandModelsVisible(false);
        return true;
    }


    // ---------------------------------------------------------------
    //                          Drop
    // ---------------------------------------------------------------

    // Drop one log in front of the player 
    public void DropOne()
    {
        if (resourceCount <= 0) return;

        SpawnWorldResource();
        RemoveShoulderModel();
        resourceCount--;

        if (resourceCount <= 0)
        {
            carryType = CarryType.None;
            inventory.SetHandModelsVisible(true);
        }

        Debug.Log($"[LogCarrySystem] Dropped one log. Carrying {resourceCount}.");
    }

    // Drop every carried log
    public void DropAll()
    {
        while (resourceCount > 0)
            DropOne();
    }

    // ---------------------------------------------------------------
    //                          Drop Input
    // ---------------------------------------------------------------

    private void HandleDropInput()
    {
        if (!IsCarrying) return;

        if (Input.GetKeyDown(dropOneKey))
        {
            holdTimer = 0f;
            holdFired = false;
        }

        if (Input.GetKey(dropOneKey))
        {
            holdTimer += Time.deltaTime;
            if (!holdFired && holdTimer >= holdDropTime)
            {
                holdFired = true;
                DropAll();
            }
        }

        if (Input.GetKeyUp(dropOneKey))
        {
            // Short tap and hold didn't already fire = drop one
            if (!holdFired)
                DropOne();

            holdTimer = 0f;
            holdFired = false;
        }
    }

    // ---------------------------------------------------------------
    //                      Shoulder Models
    // ---------------------------------------------------------------

    private void AddShoulderModel(GameObject prefab, Vector3 stackOffset)
    {
        if (prefab == null || shoulderPoint == null) return;

        // Each additional log is offset slightly so they stack visibly
        Vector3 localOffset = stackOffset * (shoulderModels.Count);
        GameObject model = Instantiate(prefab, shoulderPoint);
        model.transform.localPosition = localOffset;
        if (carryType == CarryType.Logs)
            model.transform.localRotation = Quaternion.Euler(90, 0, 0);
        else
            model.transform.localRotation = Quaternion.Euler(0, 90, 0);

        // Disable physics on the shoulder model - it just rides with the player
        Rigidbody rb = model.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity  = false;
        }

        // Disable WorldLog on the shoulder model so it cant be interacted with directly
        WorldLog wl = model.GetComponent<WorldLog>();
        if (wl != null) wl.enabled = false;
        WorldRock wr = model.GetComponent<WorldRock>();
        if (wr != null) wr.enabled = false;

        shoulderModels.Add(model);
    }

    private void RemoveShoulderModel()
    {
        if (shoulderModels.Count == 0) return;
        int last = shoulderModels.Count - 1;
        Destroy(shoulderModels[last]);
        shoulderModels.RemoveAt(last);

        // Also remove from data lists
        if (carryType == CarryType.Logs && carriedLogData.Count > 0)
            carriedLogData.RemoveAt(carriedLogData.Count - 1);
        else if (carryType == CarryType.Rocks && carriedRockData.Count > 0)
            carriedRockData.RemoveAt(carriedRockData.Count - 1);
    }

    // ---------------------------------------------------------------
    //                      Spawn Dropped Log
    // ---------------------------------------------------------------

    private void SpawnWorldResource()
    {
        Camera cam = Camera.main;
        Vector3 dropDir = cam != null ? cam.transform.forward : transform.forward;
        dropDir.y = 0f;
        dropDir.Normalize();
        Vector3 spawnPos = transform.position + dropDir * 0.9f + Vector3.up * 0.3f;
        Quaternion spawnRot = Quaternion.Euler(0f, Random.Range(0, 360), 0);

        if (carryType == CarryType.Logs)
        {
            TreeData data = carriedLogData.Count > 0 ? carriedLogData[carriedLogData.Count - 1] : null;
            GameObject prefab = data != null ? data.logPrefab : logPrefab;
            if (prefab == null) return;

            GameObject dropped = Instantiate(prefab, spawnPos, spawnRot);
            SetupRigidbody(dropped);

            // Ensure it has WorldLog so the player can pick it back up
            WorldLog wl = dropped.GetComponent<WorldLog>();
            if (wl == null)
                wl = dropped.AddComponent<WorldLog>();
            wl.enabled = true;
            wl.isPlaced = false;
            wl.treeData = data;
        }
        else if (carryType == CarryType.Rocks)
        {
            RockData data = carriedRockData.Count > 0 ? carriedRockData[carriedRockData.Count - 1] : null;
            GameObject prefab = data != null ? data.rockItemPrefab : rockPrefab;
            if (prefab == null) return;

            GameObject dropped = Instantiate(prefab, spawnPos, spawnRot);
            SetupRigidbody(dropped);

            // Ensure it has WorldLog so the player can pick it back up
            WorldRock wr = dropped.GetComponent<WorldRock>();
            if (wr == null)
                wr = dropped.AddComponent<WorldRock>();
            wr.enabled = true;
            wr.isPlaced = false;
            wr.rockData = data;
        }

    }

    private void SetupRigidbody(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null) rb = obj.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
    }

}
