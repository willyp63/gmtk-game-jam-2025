using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TooltipDirection
{
    Right,
    Left,
    Above,
    Below,
}

public class TooltipUIManager : Singleton<TooltipUIManager>
{
    [Header("Tooltip UI References")]
    public GameObject tooltipObject;
    public TextMeshProUGUI tooltipText;
    public TextMeshProUGUI tooltipTextRight;

    protected override void Awake()
    {
        base.Awake();

        HideTooltip();
    }

    public void HideTooltip()
    {
        if (tooltipObject != null)
            tooltipObject.SetActive(false);
    }

    public void ShowTooltip(string content, string contentRight = "")
    {
        if (string.IsNullOrEmpty(content))
            return;

        if (tooltipObject == null || tooltipText == null)
            return;

        // Set tooltip text content
        tooltipText.text = content;
        tooltipTextRight.text = contentRight;

        // Activate the tooltip at its initial UI position
        tooltipObject.SetActive(true);
    }
}
