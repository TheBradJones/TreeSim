using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WorldRock : MonoBehaviour
{
    [Header("Data")]
    public RockData rockData;

    [Header("Data")]
    public bool isPlaced = false;

    // ---------------------------------------------------------------
    //                      Public API
    // ---------------------------------------------------------------

    public void OnPickedUp()
    {
        Destroy(gameObject);
    }

    public bool IsPickupable => !isPlaced;
}
