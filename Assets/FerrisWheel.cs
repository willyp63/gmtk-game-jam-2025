using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FerrisWheel : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField]
    private float rotationSpeed = 90f; // degrees per second

    [SerializeField]
    private float rotationAngle = 45f; // 360° / 8 carts = 45° per cart

    private bool isRotating = false;
    private Quaternion targetRotation;

    void Start()
    {
        // Initialize the target rotation to current rotation
        targetRotation = transform.rotation;
    }

    void Update()
    {
        // Only handle input if the wheel is not currently rotating
        if (!isRotating)
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        // Check for left arrow key (rotate counter-clockwise)
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            StartRotation(-rotationAngle);
        }
        // Check for right arrow key (rotate clockwise)
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            StartRotation(rotationAngle);
        }
    }

    private void StartRotation(float angle)
    {
        if (!isRotating)
        {
            // Calculate the new target rotation
            targetRotation = transform.rotation * Quaternion.Euler(0, 0, angle);

            // Start the rotation coroutine
            StartCoroutine(RotateWheel());
        }
    }

    private IEnumerator RotateWheel()
    {
        isRotating = true;

        Quaternion startRotation = transform.rotation;
        float elapsedTime = 0f;
        float duration = Mathf.Abs(rotationAngle) / rotationSpeed;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Apply smooth easing: slow start, fast middle, slow end
            float easedT = SmoothStep(t);

            // Use smooth interpolation for the rotation
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, easedT);

            yield return null;
        }

        // Ensure we end up exactly at the target rotation
        transform.rotation = targetRotation;

        isRotating = false;
    }

    // Smooth easing function for gradual acceleration and deceleration
    private float SmoothStep(float t)
    {
        // Apply cubic easing: 3t² - 2t³
        // This creates a smooth curve with gradual start and end
        return t * t * (3f - 2f * t);
    }
}
