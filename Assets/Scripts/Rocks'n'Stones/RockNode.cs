using UnityEngine;

public class RockNode : MonoBehaviour, IHittable
{
    [Header("Data")]
    public RockData rockData;

    // ---------------------------------------------------------------
    //                          Runtime
    // ---------------------------------------------------------------

    private int currentStage = 1;   // Starts at stage 1 (full)
    private GameObject currentMesh;

    // ---------------------------------------------------------------
    //                      Unity Lifecycle
    // ---------------------------------------------------------------

    private void Start()
    {
        ShowStage(1);
    }

    // ---------------------------------------------------------------
    //                  IHittable implementation
    // ---------------------------------------------------------------

    public void OnHit(Vector3 hitPoint, Vector3 hitNormal, ToolItem tool)
    {
        if (tool == null) return;
        if (tool.toolName != "Pickaxe") return;

        currentStage++;
        Debug.Log("[RockNode] Hit! Stage {currentStage}/5");

        if (currentStage >= 5)
        {
            DestroyNode();
            return;
        }
    }

    // ---------------------------------------------------------------
    //                      Stage Management
    // ---------------------------------------------------------------

    private void ShowStage(int stage)
    {
        if (rockData == null) return;

        if (currentMesh != null)
            currentMesh.SetActive(false);

        GameObject stageMesh = GetStageMesh(stage);
        if (stageMesh != null)
        {
            stageMesh.SetActive(true);
            currentMesh = stageMesh;
        }
    }

    private GameObject GetStageMesh(int stage)
    {
        switch (stage)
        {
            case 1: return rockData.stage1;
            case 2: return rockData.stage2;
            case 3: return rockData.stage3;
            case 4: return rockData.stage4;
            default: return null;
        }
    }

    // ---------------------------------------------------------------
    //                      Destroy
    // ---------------------------------------------------------------

    private void DestroyNode()
    {
        if (rockData != null && rockData.rockItemPrefab != null)
        {
            // Spawn rock items 
            for (int i = 0; i < rockData.rocksToSpawn; i++)
            {
                GameObject rock = Instantiate(
                    rockData.rockItemPrefab, 
                    transform.position, 
                    Quaternion.Euler(0f, Random.Range(0f, 360f), 0)
                    );

                WorldRock wr = rock.GetComponent<WorldRock>();
                if (wr == null)
                    wr = rock.AddComponent<WorldRock>();
                wr.rockData = rockData;

                Rigidbody rb = rock.GetComponent<Rigidbody>();
                if (rb == null)
                    rb = rock.AddComponent<Rigidbody>();
                rb.isKinematic = false;
                rb.useGravity = true;
            }
        }

        Debug.Log("[RockNode] Rock node destroyed.");
        Destroy(gameObject);
    }

}
