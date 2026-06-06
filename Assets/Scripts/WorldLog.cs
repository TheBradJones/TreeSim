using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WorldLog : MonoBehaviour
{
    // ---------------------------------------------------------------
    //                          Inspector
    // ---------------------------------------------------------------

    [Header("State")]
    public bool isPlaced = false;   // Set to true once the log is locked into a build placement. prevenets pickup until toggled off

    [Header("Log Type")]
    public TreeData treeData;

    // ---------------------------------------------------------------
    //                      Public API
    // ---------------------------------------------------------------

    // Called by logCarrySystem when th player picks this log up.
    // Simply destroy the world object - the carry system handles adding a shoulder model on its end
    public void OnPickedUp()
    {
        Destroy(gameObject);
    }

    public bool IsPickupable => !isPlaced;
}
