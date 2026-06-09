using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LogStorage : MonoBehaviour
{
    public Transform[] logPositions;
    public int slot = 0;

    private bool doOnce = true;
    private WorldLog wl;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Log")
        {
            doOnce = false;
            Debug.Log($"[LogStorage] Log detected!  slot: {slot}");

            AddLogToStorage(other.gameObject);
        }
    }

    private void AddLogToStorage(GameObject log)
    {
        if (slot >= logPositions.Length) return;

        wl = log.GetComponent<WorldLog>();
        if (wl.isStored) return;

        wl.isStored = true;

        log.transform.SetParent(logPositions[slot]);
        log.transform.localPosition = Vector3.zero;
        log.transform.localRotation = Quaternion.identity;

        Debug.Log($"[LogStorage] BEFORE    slot: {slot}");
        slot++;
        Debug.Log($"[LogStorage] AFTER    slot: {slot}");

        Rigidbody logRb = log.GetComponent<Rigidbody>();
        logRb.isKinematic = true;
        logRb.useGravity = false;

        Debug.Log($"[LogStorage] FINISHED    slot: {slot}");
    }

    public void RemoveLogFromStorage()
    {
        slot--;
        Debug.Log($"[LogStorage] REMOVING    slot: {slot}");
    }
}
