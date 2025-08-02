using UnityEngine;

public class HueCycler : MonoBehaviour
{
    [Header("Hue Cycling Settings")]
    public float cycleDuration = 10f;

    public float saturation = 1f;

    public float lightness = 0.5f;

    public float startHue = 0f;

    private SpriteRenderer spriteRenderer;
    private float currentHue = 0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("HueCycler requires a SpriteRenderer component!");
        }

        currentHue = startHue;
    }

    void Update()
    {
        if (spriteRenderer != null)
        {
            // Increment hue over time
            currentHue += Time.deltaTime / cycleDuration;

            // Keep hue in 0-1 range
            if (currentHue > 1f)
                currentHue -= 1f;

            // Convert HSV to RGB and apply to sprite
            Color newColor = Color.HSVToRGB(currentHue, saturation, lightness);
            spriteRenderer.color = newColor;
        }
    }
}
