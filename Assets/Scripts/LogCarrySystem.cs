using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerInventory))]
public class LogCarrySystem : MonoBehaviour
{
    // ---------------------------------------------------------------
    //                          Inspector
    // ---------------------------------------------------------------

    [Header("Carry Settings")]
    public int maxLogs = 3;

    [Header("Shoulder Mount")]
    public Transform shoulderPoint; // Logs are parented here visually

    public Vector3 logStackOffset = new Vector3(0, 1, 0);   // Local space offset applied per extra log so they dont all overlap on the shoulder

    [Header("Drop Settings")]
    public KeyCode dropOneKey = KeyCode.G;
    public float holdDropTime = 1f;   // Seconds to hold G before dropping all

    [Header("Log Prefab")]
    public GameObject logPrefab;    // Prefab spawned in the world a log is dropped. must have a rb and worldlog component

    // ---------------------------------------------------------------
    //                          Runtime
    // ---------------------------------------------------------------

    private PlayerInventory inventory;
    private readonly List<GameObject> shoulderModels = new List<GameObject>();
    private int logCount = 0;

    private float holdTimer = 0f;
    private bool holdFired = false;

    public int LogCount => logCount;
    public bool IsCarrying => logCount > 0;
    public bool CanCarryMore => logCount < maxLogs;

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
    //                      Public API
    // ---------------------------------------------------------------

    public bool TryPickupLog(WorldLog worldLog)
    {
        if (worldLog == null) return false;
        if (!CanCarryMore)
        {
            Debug.Log("[LogCarrySystem] Already at max logs.");
            return false;
        }

        // Remove the world object
        worldLog.OnPickedUp();

        logCount++;
        AddShoulderModel();
        inventory.SetHandModelsVisible(false);

        Debug.Log($"[LogCarrySystem] Picked up log. Carrying {logCount}/{maxLogs}.");
        return true;
    }


    // ---------------------------------------------------------------
    //                          Drop
    // ---------------------------------------------------------------

    // Drop one log in front of the player 
    public void DropOne()
    {
        if (logCount <= 0) return;

        SpawnWorldLog();
        RemoveShoulderModel();
        logCount--;

        if (!IsCarrying)
            inventory.SetHandModelsVisible(true);

        Debug.Log($"[LogCarrySystem] Dropped one log. Carrying {logCount}/{maxLogs}.");
    }

    // Drop every carried log
    public void DropAll()
    {
        while (logCount > 0)
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

    private void AddShoulderModel()
    {
        if (logPrefab == null || shoulderPoint == null) return;

        // Each additional log is offset slightly so they stack visibly
        Vector3 localOffset = logStackOffset * (shoulderModels.Count);
        Debug.Log($"[LogCarrySystem] Shoulder offset for log {shoulderModels.Count}: {localOffset}");
        GameObject model = Instantiate(logPrefab, shoulderPoint);
        model.transform.localPosition = localOffset;
        model.transform.localRotation = Quaternion.identity;

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

        shoulderModels.Add(model);
    }

    private void RemoveShoulderModel()
    {
        if (shoulderModels.Count == 0) return;
        int last = shoulderModels.Count - 1;
        Destroy(shoulderModels[last]);
        shoulderModels.RemoveAt(last);
    }

    // ---------------------------------------------------------------
    //                      Spawn Dropped Log
    // ---------------------------------------------------------------

    private void SpawnWorldLog()
    {
        if (logPrefab == null) return;

        // Place slightly in front of the player at ground level
        Camera cam = Camera.main;
        Vector3 dropDir = cam != null ? cam.transform.forward : transform.forward;
        dropDir.y = 0f;
        Vector3 spawnPos = transform.position + dropDir * 0.9f + Vector3.up * 0.3f;   // Small lift so rb doesnt clip on floor

        // Random yaw so dropped logs dont all look identical
        Quaternion spawnRot = Quaternion.Euler(0f, Random.Range(0, 360), 0f);

        GameObject dropped = Instantiate(logPrefab, spawnPos, spawnRot);

        // Ensure it has physics so it settles on the ground
        Rigidbody rb = dropped.GetComponent<Rigidbody>();
        if (rb == null)
            rb = dropped.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;

        // Ensure it has WorldLog so the player can pick it back up
        WorldLog wl = dropped.GetComponent<WorldLog>();
        if (wl == null)
            wl = dropped.AddComponent<WorldLog>();
        wl.enabled = true;
        wl.isPlaced = false;
    }

}
