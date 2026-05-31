using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeavyItemUI : MonoBehaviour
{
    [Header("References")]
    public Image logIcon;
    public TMP_Text logCount;

    private LogCarrySystem logCarry;

    private void Awake()
    {
        logCarry = FindFirstObjectByType<LogCarrySystem>();
    }

    private void Update()
    {
        if (logCarry == null) return;

        bool isCarrying = logCarry.IsCarrying;

        logIcon.gameObject.SetActive(isCarrying);

        if (isCarrying)
            logCount.text = logCarry.logCount.ToString();
    }
}
