using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeavyItemUI : MonoBehaviour
{
    [Header("References")]
    public Image icon;
    public TMP_Text count;

    [Header("Sprites")]
    public Sprite logIcon;
    public Sprite stoneIcon;

    private LogCarrySystem logCarry;

    private void Awake()
    {
        logCarry = FindFirstObjectByType<LogCarrySystem>();
    }

    private void Update()
    {
        if (logCarry == null) return;

        bool isCarrying = logCarry.IsCarrying;

        

        if (isCarrying)
        {
            // Check if carrying log or stone   then   icon.sprite = logIcon : stoneIcon
            count.gameObject.SetActive(true);
            count.text = logCarry.logCount.ToString();
        }
        else
            count.gameObject.SetActive(false);
    }
}
