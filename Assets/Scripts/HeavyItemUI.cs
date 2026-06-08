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

    private ResourceCarrySystem rcs;

    private void Awake()
    {
        rcs = FindFirstObjectByType<ResourceCarrySystem>();
    }

    private void Update()
    {
        if (rcs == null) return;

        bool isCarrying = rcs.IsCarrying;


        if (isCarrying)
        {
            icon.gameObject.SetActive(true);
            count.gameObject.SetActive(true);

            icon.sprite = rcs.carryType == CarryType.Rocks ? stoneIcon : logIcon;   // Switch between rock and log icon 

            count.text = rcs.resourceCount.ToString();
        }
        else
        {
            icon.gameObject.SetActive(false);
            count.gameObject.SetActive(false);
        }
    }
}
