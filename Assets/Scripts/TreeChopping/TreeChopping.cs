using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.tvOS;

[RequireComponent(typeof(Rigidbody))]
public class TreeChopping : MonoBehaviour, IHittable
{
    [Header("Chunks")]
    public TreeChunk[] chunks;

    [Header("Fall Settings")]
    public int chunksToFall = 6;        // How many chunks must be removed in order for it to fall
    public float fallDuration = 2.5f;   // How long the fall takes in seconds
    public float fallForce = 50f;      // Force applied when trees fall

    [Header("Tree Parts")]
    public GameObject fullTreeMesh;     // Hide this on fall
    public GameObject stump;            // Always stays
    public GameObject upper;            // Detach on fall

    [Header("Adjacent Chunk Bleed")]
    public bool bleedToNeighbours = true;    // if true, hitting a chunk also damages its immediate neighbours slightly

    // ---------------------------------------------------------------
    //                          Runtime
    // ---------------------------------------------------------------

    private Rigidbody rb;
    private bool isFalling = false;
    private int removedCount = 0;

    // ---------------------------------------------------------------
    //                      Unity Lifecycle
    // ---------------------------------------------------------------

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // keep tree static until it falls
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    // ---------------------------------------------------------------
    //                  IHittable implementation
    // ---------------------------------------------------------------

    public void OnHit(Vector3 hitPoint, Vector3 hitNormal, ToolItem tool)
    {
        if (isFalling) return;

        if (tool == null) return;

        TreeChunk nearest = GetNearestChunk(hitPoint);
        if (nearest == null) return;

        if (!nearest.isRemoved)
        {
            RemoveChunk(nearest);

            // Bleed to immediate neighbours
            if (bleedToNeighbours)
                BleedToNeighbours(nearest);
        }
        else
        {
            // Chunk already gone - bleed to neighbours instead
            bool bled = false;
            if (bleedToNeighbours)
                bled = BleedToNeighbours(nearest);

            if (!bled)
            {
                // Find next nearest intact chunk
                TreeChunk next = GetNearestIntactChunk(hitPoint);
                if (next != null) RemoveChunk(next);
            }
        }

        Debug.Log($"[TreeChopping] Removed: {removedCount}/{chunksToFall}");

        if (removedCount >= chunksToFall)
            StartCoroutine(FallTree());
    }

    // ---------------------------------------------------------------
    // Chunk removal
    // ---------------------------------------------------------------

    private void RemoveChunk(TreeChunk chunk)
    {
        if (chunk.isRemoved) return;
        chunk.Remove();
        removedCount++;
    }

    private bool BleedToNeighbours(TreeChunk chunk)
    {
        int index = System.Array.IndexOf(chunks, chunk);
        if (index == -1) return false;

        int left = (index - 1 + chunks.Length) % chunks.Length;
        int right = (index + 1) % chunks.Length;

        // Only bleed to intact neighbours dont double count
        bool removed = false;
        if (!chunks[left].isRemoved)    { RemoveChunk(chunks[left]); removed = true; }
        if (!chunks[right].isRemoved)   { RemoveChunk(chunks[right]); removed = true; }
        return removed;
    }

    // ---------------------------------------------------------------
    // Nearest chunk helpers
    // ---------------------------------------------------------------

    private TreeChunk GetNearestChunk(Vector3 hitPoint)
    {
        // Compare on XZ plane only - height doesnt matter for ring detection
        TreeChunk nearest = null;
        float minDist = float.MaxValue;

        foreach (TreeChunk chunk in chunks)
        {
            Vector2 chunkXZ = new Vector2(chunk.WorldPosition.x, chunk.WorldPosition.z);
            Vector2 hitXZ   = new Vector2(hitPoint.x, hitPoint.z);
            float dist = Vector2.Distance(chunkXZ, hitXZ);

            if (dist < minDist)
            {
                minDist = dist;
                nearest = chunk;
            }
        }

        return nearest;
    }

    private TreeChunk GetNearestIntactChunk(Vector3 hitPoint)
    {
        TreeChunk nearest = null;
        float minDist = float.MaxValue;

        foreach (TreeChunk chunk in chunks)
        {
            if (chunk.isRemoved) continue;

            Vector2 chunkXZ = new Vector2(chunk.WorldPosition.x, chunk.WorldPosition.z);
            Vector2 hitXZ = new Vector2(hitPoint.x, hitPoint.z);
            float dist = Vector2.Distance(chunkXZ, hitXZ);

            if (dist < minDist)
            {
                minDist = dist;
                nearest = chunk;
            }
        }

        return nearest;
    }

    // ---------------------------------------------------------------
    // Fall
    // ---------------------------------------------------------------

    private IEnumerator FallTree()
    {
        isFalling = true;
        Debug.Log("[TreeChopping] Tree is falling!");

        // Calculate fall direction - weighted average of removed chunk positions
        Vector3 fallDirection = CalculateFallDirection();

        // Disable chunk collider so they dont fight the fall
        foreach (TreeChunk chunk in chunks)
        {
            Collider col = chunk.GetComponentInChildren<Collider>();
            if (col != null) col.enabled = false;
        }

        // Detach stump BEFORE physics so it stays rooted
        if (stump != null)
        {
            stump.transform.SetParent(null);
            stump.SetActive(true);
        }

        // Detach upper and give it its own physics
        if (upper != null)
        {
            upper.transform.SetParent(null);

            Collider trunkCol = GetComponent<Collider>();
            Collider upperCol = upper.GetComponent<Collider>();
            if (trunkCol != null && upperCol != null)
                Physics.IgnoreCollision(trunkCol, upperCol);

            Rigidbody upperRb = upper.GetComponent<Rigidbody>();
            if (upperRb == null)
                upperRb = upper.AddComponent<Rigidbody>();
            upperRb.isKinematic = false;
            upperRb.useGravity = true;
            upperRb.AddForce(fallDirection * (fallForce * 0.005f), ForceMode.Impulse);
        }

        // Enable Physics
        rb.isKinematic = false;
        rb.useGravity = true;

        // Apply force in fall direction at the top of the tree
        Vector3 forcePoint = transform.position + Vector3.up * 4;
        rb.AddForceAtPosition(fallDirection * fallForce, forcePoint, ForceMode.Impulse);

        // Wait for fall to complete
        yield return new WaitForSeconds(fallDuration + 1f);

        // Hide full tree
        if (fullTreeMesh != null) 
            fullTreeMesh.SetActive(false);

        yield return new WaitForSeconds(5f);    // Wait for fall to complete
        Destroy(upper);
        // Instantiate logs for pickup

        // Disable this script - tree is done
        this.enabled = false;
    }

    private Vector3 CalculateFallDirection()
    {
        Vector3 damageCenter = Vector3.zero;
        int count = 0;

        foreach (TreeChunk chunk in chunks)
        {
            if (chunk.isRemoved)
            {
                damageCenter += chunk.WorldPosition;
                count++;
            }
        }

        if (count == 0) return transform.forward;

        damageCenter /= count;

        // Fall TOWARD the damage center (most chopped side)
        Vector3 dir = (damageCenter - transform.position);
        dir.y = 0f;
        return dir.normalized;

    }
}
