using UnityEngine;
using UnityEngine.Events;

public class StaminaSystem : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float staminaCostPerSwing = 15f;
    public float regenRate = 5f;
    public float regenDelay = 1.5f;

    [Header("Events")]
    public UnityEvent<float, float> onStaminaChanged;   // current, max
    public UnityEvent onStaminaEmpty;

    // ---------------------------------------------------------------
    //                          Runtime
    // ---------------------------------------------------------------

    private float current;
    private float regenTimer;
    private bool regenPaused;

    public float Current => current;
    public float Max => maxStamina;
    public bool HasStamina => current >= staminaCostPerSwing;

    // ---------------------------------------------------------------
    //                      Unity lifecycle
    // ---------------------------------------------------------------

    private void Awake()
    {
        current = maxStamina;
    }

    private void Update()
    {
        HandleRegen();
    }

    // ---------------------------------------------------------------
    // Public API
    // ---------------------------------------------------------------

    public bool TryConsume()
    {
        if (!HasStamina)
        {
            onStaminaEmpty?.Invoke();
            return false;
        }

        current = Mathf.Max(0f, current - staminaCostPerSwing);
        regenTimer = 0f;
        regenPaused = true;

        onStaminaChanged?.Invoke(current, maxStamina);
        return true;
    }

    // ---------------------------------------------------------------
    // Private
    // ---------------------------------------------------------------
    
    private void HandleRegen()
    {
        if (current >= maxStamina) return;

        if (regenPaused)
        {
            regenTimer += Time.deltaTime;
            if (regenTimer >= regenDelay)
                regenPaused = false;
            return;
        }

        current = Mathf.Min(maxStamina, current + regenRate * Time.deltaTime);
        onStaminaChanged?.Invoke(current, maxStamina);
    }



}
