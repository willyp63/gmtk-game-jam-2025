using TMPro;
using UnityEngine;

public class PointsIndicator : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI pointsText;

    [SerializeField]
    private ShakeBehavior shakeBehavior;

    private Cart targetCart;
    private float xOffset = 0f;
    private float yOffset = 0f;
    private int points = 0;

    private Camera mainCamera;
    private Canvas worldSpaceCanvas;

    public void Initialize(Cart cart, Canvas canvas, float xOffset = 0f, float yOffset = 2f)
    {
        points = 0;
        targetCart = cart;
        worldSpaceCanvas = canvas;
        this.xOffset = xOffset;
        this.yOffset = yOffset;
        mainCamera = Camera.main;

        if (pointsText == null)
        {
            pointsText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    public void UpdatePosition()
    {
        if (targetCart.Hinge == null || worldSpaceCanvas == null || mainCamera == null)
            return;

        // Calculate the world position above the hinge
        Vector3 worldPosition =
            targetCart.Hinge.position + Vector3.up * yOffset + Vector3.right * xOffset;

        // Convert world position to screen position
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        // Convert screen position to canvas position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            worldSpaceCanvas.transform as RectTransform,
            screenPosition,
            worldSpaceCanvas.worldCamera,
            out Vector2 localPoint
        );

        // Update the indicator position
        transform.localPosition = localPoint;
    }

    public void UpdatePoints()
    {
        Animal animal = targetCart.CurrentAnimal;
        if (animal != null)
        {
            pointsText.text = animal.CurrentPoints.ToString();

            if (points != animal.CurrentPoints)
            {
                points = animal.CurrentPoints;
                shakeBehavior.Shake();
            }
        }
        else
        {
            pointsText.text = "";
            points = -1;
        }
    }
}
