using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightManager : Singleton<LightManager>
{
    [SerializeField]
    private Light2D globalLight;

    [SerializeField]
    private List<Light2D> lights;

    [SerializeField]
    private Color morningLightColor = new Color(1f, 0.9f, 0.7f, 1f); // Warm morning light

    [SerializeField]
    private Color middayLightColor = new Color(1f, 1f, 1f, 1f); // Bright white light

    [SerializeField]
    private Color nightLightColor = new Color(0.2f, 0.3f, 0.8f, 1f); // Cool blue night light

    [SerializeField]
    private float morningLightIntensity = 0.8f;

    [SerializeField]
    private float middayLightIntensity = 1.0f;

    [SerializeField]
    private float nightLightIntensity = 0.3f;

    [SerializeField]
    private float morningTime = 0.1f; // From 0 to morningTime is purely morning light

    [SerializeField]
    private float middayStartTime = 0.4f; // From morningTime to middayStartTime is transition from morning to midday

    [SerializeField]
    private float middayEndTime = 0.6f; // From middayStartTime to middayEndTime is purely midday light

    [SerializeField]
    private float nightTime = 0.7f; // From middayEndTime to nightTime is transition from midday to night & from nightTime to 1 is purely night light

    [SerializeField]
    private float lightsOnTime = 0.75f; // Time when lights turn on

    private bool lightsAreOn = false;

    public void UpdateLighting(float timeRatio, float transitionDuration)
    {
        if (globalLight == null)
            return;

        // Determine lighting based on energy ratio
        Color targetColor;
        float targetIntensity;

        if (timeRatio <= morningTime)
        {
            // Morning light
            targetColor = morningLightColor;
            targetIntensity = morningLightIntensity;
        }
        else if (timeRatio <= middayStartTime)
        {
            // Interpolate between morning and midday
            float t = (timeRatio - morningTime) / (middayStartTime - morningTime);
            targetColor = Color.Lerp(morningLightColor, middayLightColor, t);
            targetIntensity = Mathf.Lerp(morningLightIntensity, middayLightIntensity, t);
        }
        else if (timeRatio <= middayEndTime)
        {
            // Midday light
            targetColor = middayLightColor;
            targetIntensity = middayLightIntensity;
        }
        else if (timeRatio <= nightTime)
        {
            // Interpolate between midday and night
            float t = (timeRatio - middayEndTime) / (nightTime - middayEndTime);
            targetColor = Color.Lerp(middayLightColor, nightLightColor, t);
            targetIntensity = Mathf.Lerp(middayLightIntensity, nightLightIntensity, t);
        }
        else
        {
            // Night light
            targetColor = nightLightColor;
            targetIntensity = nightLightIntensity;
        }

        // Smoothly transition the global light
        StartCoroutine(TransitionGlobalLight(targetColor, targetIntensity, transitionDuration));

        // Handle individual lights
        bool shouldTurnOnLights = timeRatio <= lightsOnTime;
        if (shouldTurnOnLights && !lightsAreOn)
        {
            SetLightsActive(true);
            lightsAreOn = true;
        }
        else if (!shouldTurnOnLights && lightsAreOn)
        {
            SetLightsActive(false);
            lightsAreOn = false;
        }
    }

    private IEnumerator TransitionGlobalLight(
        Color targetColor,
        float targetIntensity,
        float duration
    )
    {
        Color startColor = globalLight.color;
        float startIntensity = globalLight.intensity;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            globalLight.color = Color.Lerp(startColor, targetColor, t);
            globalLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);

            yield return null;
        }

        // Ensure we reach the exact target values
        globalLight.color = targetColor;
        globalLight.intensity = targetIntensity;
    }

    public void SetLightsActive(bool active)
    {
        foreach (var light in lights)
        {
            if (light != null)
            {
                light.enabled = active;
            }
        }
    }
}
