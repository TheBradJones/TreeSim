using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StaminaUI : MonoBehaviour
{
    [Header("References")]
    public Image fillImage;
    public TextMeshProUGUI label;

    [Header("Colors")]
    public Color highColor = new Color(0.2f, 0.85f, 0.3f);
    public Color midColor = new Color(0.95f, 0.75f, 0.1f);
    public Color lowColor = new Color(0.9f, 0.2f, 0.15f);

    // ---------------------------------------------------------------
    // Called by StaminaSystem.onStaminaChanged event
    // ---------------------------------------------------------------

    public void OnStaminaChanged(float current, float max)
    {
        if (fillImage == null) return;

        float ratio = current / max;
        fillImage.fillAmount = ratio;
        fillImage.color = ratio > 0.5f ? highColor :
                          ratio > 0.25f ? midColor : lowColor;

        if (label != null)
            label.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }
}
