using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField]
    private FerrisWheel ferrisWheel;

    [SerializeField]
    private GameObject pointsIndicatorPrefab;

    [SerializeField]
    private Canvas worldSpaceCanvas;

    [SerializeField]
    private float yOffset = 2f;

    private List<PointsIndicator> pointsIndicators = new List<PointsIndicator>();

    private void Start()
    {
        InitializePointsIndicators();
    }

    private void Update()
    {
        UpdatePointsIndicators();
    }

    private void InitializePointsIndicators()
    {
        if (ferrisWheel == null || pointsIndicatorPrefab == null || worldSpaceCanvas == null)
        {
            Debug.LogError("UIManager: Missing required references!");
            return;
        }

        // Clear any existing indicators
        foreach (var indicator in pointsIndicators)
        {
            if (indicator != null)
            {
                Destroy(indicator.gameObject);
            }
        }
        pointsIndicators.Clear();

        // Get all cart hinges from the FerrisWheel
        List<Cart> carts = ferrisWheel.Carts;

        // Create a points indicator for each cart hinge
        foreach (Cart cart in carts)
        {
            if (cart.Hinge != null)
            {
                GameObject indicatorObject = Instantiate(
                    pointsIndicatorPrefab,
                    worldSpaceCanvas.transform
                );
                PointsIndicator indicator = indicatorObject.GetComponent<PointsIndicator>();

                if (indicator != null)
                {
                    indicator.Initialize(cart, worldSpaceCanvas, yOffset);
                    pointsIndicators.Add(indicator);
                }
            }
        }
    }

    private void UpdatePointsIndicators()
    {
        foreach (var indicator in pointsIndicators)
        {
            if (indicator != null)
            {
                indicator.UpdatePosition();
                indicator.UpdatePoints();
            }
        }
    }
}
