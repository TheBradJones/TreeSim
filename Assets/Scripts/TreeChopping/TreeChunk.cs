using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class TreeChunk : MonoBehaviour
{
    [Header("Visual")]
    public GameObject chunkMesh;        // The chunk mesh that disapears when this zone is hit  +   can be cube for now, swap for probuilder wedge later

    [Header("State")]
    public bool isRemoved = false;

    // ---------------------------------------------------------------
    // Public API
    // ---------------------------------------------------------------

    public void Remove()
    {
        if (isRemoved) return;
        isRemoved = true;

        if (chunkMesh != null)
            chunkMesh.SetActive(false);

        Debug.Log($"[TreeChunk] Chunk removed: {gameObject.name}");
    }

    public Vector3 WorldPosition => transform.position;

}
