using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MarqueeSign : MonoBehaviour
{
    [Header("Marquee Settings")]
    [SerializeField]
    private float waveSpeed = 2f;

    [SerializeField]
    private float maxIntensity = 1f;

    [SerializeField]
    private float minIntensity = 0.1f;

    [SerializeField]
    private float waveLength = 3f;

    [SerializeField]
    private bool randomizeStartOffset = true;

    [Header("Color Animation (Optional)")]
    [SerializeField]
    private bool animateColor = false;

    [SerializeField]
    private Color[] colorSequence = new Color[]
    {
        Color.white,
        Color.yellow,
        Color.red,
        Color.blue,
    };

    [SerializeField]
    private float colorChangeSpeed = 1f;

    [Header("Pattern Options")]
    [SerializeField]
    private MarqueePattern pattern = MarqueePattern.Wave;

    [SerializeField]
    private bool reverseDirection = false;

    public enum MarqueePattern
    {
        Wave, // Smooth sine wave
        Chase, // One light at a time
        Pulse, // All lights pulse together
        Random, // Random flashing
        Alternating, // Every other light
    }

    private Light2D[] lights;
    private float[] lightOffsets;
    private Color[] originalColors;
    private float time;

    void Start()
    {
        CollectLights();
        InitializeLights();
    }

    void CollectLights()
    {
        // Find all Light2D components in children recursively
        lights = GetComponentsInChildren<Light2D>();

        if (lights.Length == 0)
        {
            Debug.LogWarning("MarqueeLights: No Light2D components found in children!");
            return;
        }

        Debug.Log($"MarqueeLights: Found {lights.Length} lights to animate");

        // Initialize arrays
        lightOffsets = new float[lights.Length];
        originalColors = new Color[lights.Length];
    }

    void InitializeLights()
    {
        for (int i = 0; i < lights.Length; i++)
        {
            // Store original colors
            originalColors[i] = lights[i].color;

            // Set up offsets based on pattern
            switch (pattern)
            {
                case MarqueePattern.Wave:
                case MarqueePattern.Chase:
                    lightOffsets[i] = randomizeStartOffset
                        ? Random.Range(0f, Mathf.PI * 2f)
                        : (i * waveLength) / lights.Length * Mathf.PI * 2f;
                    break;

                case MarqueePattern.Alternating:
                    lightOffsets[i] = (i % 2) * Mathf.PI;
                    break;

                case MarqueePattern.Random:
                    lightOffsets[i] = Random.Range(0f, Mathf.PI * 2f);
                    break;

                case MarqueePattern.Pulse:
                    lightOffsets[i] = 0f;
                    break;
            }
        }
    }

    void Update()
    {
        if (lights == null || lights.Length == 0)
            return;

        time += Time.deltaTime;

        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] == null)
                continue;

            float intensity = CalculateIntensity(i);
            Debug.Log($"MarqueeLights: Light {i} intensity: {intensity}");
            lights[i].intensity = intensity;

            if (animateColor && colorSequence.Length > 0)
            {
                Color newColor = CalculateColor(i);
                lights[i].color = newColor;
            }
        }
    }

    float CalculateIntensity(int lightIndex)
    {
        float direction = reverseDirection ? -1f : 1f;
        float currentTime = time * waveSpeed * direction;

        float adjustedMinIntensity =
            lights[lightIndex].gameObject.name == "Light Center"
                ? minIntensity * 20f
                : minIntensity;
        float adjustedMaxIntensity =
            lights[lightIndex].gameObject.name == "Light Center"
                ? maxIntensity * 20f
                : maxIntensity;

        switch (pattern)
        {
            case MarqueePattern.Wave:
                return Mathf.Lerp(
                    adjustedMinIntensity,
                    adjustedMaxIntensity,
                    (Mathf.Sin(currentTime + lightOffsets[lightIndex]) + 1f) * 0.5f
                );

            case MarqueePattern.Chase:
                // Create a traveling peak
                float chasePosition = (currentTime + lightOffsets[lightIndex]) % (Mathf.PI * 2f);
                float chasePeak = Mathf.Exp(-Mathf.Pow(chasePosition - Mathf.PI, 2f) * 2f);
                return Mathf.Lerp(adjustedMinIntensity, adjustedMaxIntensity, chasePeak);

            case MarqueePattern.Pulse:
                return Mathf.Lerp(
                    adjustedMinIntensity,
                    adjustedMaxIntensity,
                    (Mathf.Sin(currentTime * 2f) + 1f) * 0.5f
                );

            case MarqueePattern.Random:
                // Use Perlin noise for smooth random variation
                return Mathf.Lerp(
                    adjustedMinIntensity,
                    adjustedMaxIntensity,
                    Mathf.PerlinNoise(currentTime + lightOffsets[lightIndex], 0f)
                );

            case MarqueePattern.Alternating:
                return Mathf.Lerp(
                    adjustedMinIntensity,
                    adjustedMaxIntensity,
                    (Mathf.Sin(currentTime + lightOffsets[lightIndex]) + 1f) * 0.5f
                );

            default:
                return adjustedMaxIntensity;
        }
    }

    Color CalculateColor(int lightIndex)
    {
        if (colorSequence.Length == 0)
            return originalColors[lightIndex];

        float colorTime = time * colorChangeSpeed + lightOffsets[lightIndex];
        float normalizedTime = (colorTime % colorSequence.Length);

        int currentIndex = Mathf.FloorToInt(normalizedTime);
        int nextIndex = (currentIndex + 1) % colorSequence.Length;
        float t = normalizedTime - currentIndex;

        return Color.Lerp(colorSequence[currentIndex], colorSequence[nextIndex], t);
    }
}
