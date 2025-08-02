using TMPro;
using UnityEngine;

public class TooltipUIManager : Singleton<TooltipUIManager>
{
    [Header("Tooltip UI References")]
    public TextMeshProUGUI tooltipText;

    protected override void Awake()
    {
        base.Awake();

        HideTooltip();
    }

    public void HideTooltip()
    {
        tooltipText.text = "";
    }

    public void ShowTooltip(string content)
    {
        tooltipText.text = content;
    }
}
