using UnityEngine;

[RequireComponent(typeof(StaminaSystem))]
public class ToolSwing : MonoBehaviour
{
    [Header("Swing Settings")]
    public float swingCooldown = 0.6f;  // Min time between swings
    public float swingDuration = 0.35f; // How long one swing motion takes
    public float hitRange = 2.5f;       // Raycast range for hit direction
    public LayerMask hitLayers = ~0;    // What the tool can hit

    [Header("Procedural Swing Motion")]
    public Vector3 restPosition = new Vector3(0.4f, -0.3f, 0.6f);   // Handpoint local position at rest
    public Vector3 swingPosition = new Vector3(0.4f, 0.2f, 0.5f);   // Handpoint local position at swing apex
    public Vector3 restRotation = new Vector3(0, 0, 0);             // Handpoint local rotation at rest (euler)
    public Vector3 swingRotation = new Vector3(50, 0, 0);           // Handpoint local rotation at swing apex (euler)

    [Header("References")]
    public Transform handPoint;     // The HandPoint transform from PlayerInventory
    public Camera playerCamera;     // Leave null to use Camera.main

    // ---------------------------------------------------------------
    //     Animator hook (uncomment when character model is added)
    // ---------------------------------------------------------------

    // [Header("Animation")]
    // public Animator characterAnimator;
    // public string swingTriggerName = "Swing";

    // ---------------------------------------------------------------
    // Runtime
    // ---------------------------------------------------------------

    private StaminaSystem stamina;
    private float cooldownTimer;
    private bool isSwinging;
    private float swingTimer;
    private bool hitRegistered;     // Only one hit per swing

    public bool IsSwinging => IsSwinging;

    // ---------------------------------------------------------------
    // Unity lifecycle
    // ---------------------------------------------------------------

    private void Awake()
    {
        stamina = GetComponent<StaminaSystem>();
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (handPoint != null)
        {
            handPoint.localPosition = restPosition;
            handPoint.localRotation = Quaternion.Euler(restPosition);
        }
    }

    private void Update()
    {
        cooldownTimer += Time.deltaTime;

        HandleInput();
        UpdateSwingMotion();
    }

    // ---------------------------------------------------------------
    // Input
    // ---------------------------------------------------------------

    private void HandleInput()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (isSwinging) return;
        if (cooldownTimer < swingCooldown) return;

        // Only swing if a tool is held
        PlayerInventory inv = GetComponent<PlayerInventory>();
        if (inv == null || inv.HeldItem == null || inv.HeldItem.toolName != "Axe" && inv.HeldItem.toolName != "Chainsaw")
        {
            Debug.Log("[ToolSwing] Axe not found - cannot begin swing");
            return;
        }

        if (!stamina.TryConsume()) return;

        BeginSwing();
    }

    // ---------------------------------------------------------------
    // Swing logic
    // ---------------------------------------------------------------

    private void BeginSwing()
    {
        isSwinging = true;
        swingTimer = 0f;
        hitRegistered = false;
        cooldownTimer = 0f;

        // Uncomment when Animator is available
        // if (characterAnimator != null)
        //     characterAnimator.SetTrigger(swingTriggerName);

        Debug.Log("[ToolSwing] Swing started");
    }

    private void UpdateSwingMotion()
    {
        if (!isSwinging || handPoint == null) return;

        swingTimer += Time.deltaTime;
        float t = swingTimer / swingDuration;

        if (t >= 1f)
        {
            // Swing Complete - return to rest
            handPoint.localPosition = restPosition;
            handPoint.localRotation = Quaternion.Euler(restRotation);
            isSwinging = false;
            return;
        }

        // Arc: swing forward (0->0.5) then back (0.5->1)
        float arc = Mathf.Sin(t * Mathf.PI); // 0 -> 1 -> 0

        handPoint.localPosition = Vector3.Lerp(restPosition, swingPosition, arc);
        handPoint.localRotation = Quaternion.Lerp(Quaternion.Euler(restRotation), Quaternion.Euler(swingRotation), arc);

        // Register hit at apex (t ~= 0.5)
        if (!hitRegistered && t >= 0.45f)
        {
            hitRegistered = true;
            RegisterHit();
        }
    }


    // ---------------------------------------------------------------
    // Hit detection
    // ---------------------------------------------------------------

    private void RegisterHit()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, hitRange, hitLayers))
        {
            Debug.Log($"[ToolSwing] Hit: {hit.collider.gameObject.name} at {hit.point}");

            // Notify any IHittable on the object
            IHittable hittable = hit.collider.GetComponent<IHittable>();
            hittable?.OnHit(hit.point, hit.normal, GetComponent<PlayerInventory>()?.HeldItem);
        }
        else
        {
            Debug.Log("[ToolSwing] Swing Missed");
        }
    }

}
