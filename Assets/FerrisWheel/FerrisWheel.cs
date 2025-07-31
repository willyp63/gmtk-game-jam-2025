using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FerrisWheel : MonoBehaviour
{
    [SerializeField]
    private List<Cart> carts = new(); // Cart #1 is initially at the top. Carts are in clockwise order.

    [SerializeField]
    private Bounds loadingZone;
    public Bounds LoadingZone => loadingZone;

    [SerializeField]
    private Bounds unloadingZone;
    public Bounds UnloadingZone => unloadingZone;

    [SerializeField]
    private float rotationSpeed = 90f; // degrees per second

    [SerializeField]
    private float rotationAngle = 45f; // 360° / 8 carts = 45° per cart

    private int topCartIndex = 0;
    private bool isRotating = false;

    public bool CanLoadAnimal()
    {
        return !isRotating && GetBottomCart().IsEmpty;
    }

    public void LoadAnimal(Animal animal)
    {
        Cart bottomCart = GetBottomCart();
        if (bottomCart.IsEmpty)
        {
            bottomCart.LoadAnimal(animal);

            animal.OnDragToUnloadingZone += () =>
            {
                if (animal.AssignedCart == GetBottomCart())
                {
                    animal.AssignedCart.UnloadAnimal();
                    Destroy(animal.gameObject);
                }
            };
        }
    }

    public Cart GetTopCart()
    {
        return carts[topCartIndex];
    }

    public Cart GetAdjacentCart(bool isClockwise)
    {
        int index = (topCartIndex + (isClockwise ? 1 : -1) + carts.Count) % carts.Count;
        return carts[index];
    }

    public Cart GetBottomCart()
    {
        return GetOppositeCart(GetTopCart());
    }

    public Cart GetOppositeCart(Cart cart)
    {
        int index = carts.IndexOf(cart);
        return carts[(index + carts.Count / 2) % carts.Count];
    }

    public void RotateWheel(bool isClockwise)
    {
        if (isRotating)
            return;

        StartCoroutine(Rotate(isClockwise));
    }

    private IEnumerator Rotate(bool isClockwise)
    {
        isRotating = true;

        // Update the dragable status of the animals
        foreach (Cart cart in carts)
        {
            cart.CurrentAnimal?.SetDragable(false);
        }

        float angle = isClockwise ? rotationAngle : -rotationAngle;
        Quaternion targetRotation = transform.rotation * Quaternion.Euler(0, 0, angle);

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

        // Update the top cart index
        topCartIndex = (topCartIndex + (isClockwise ? 1 : -1) + carts.Count) % carts.Count;
        Debug.Log($"Top cart index: {topCartIndex}");

        // Update the dragable status of the animals
        Cart bottomCart = GetBottomCart();
        foreach (Cart cart in carts)
        {
            cart.CurrentAnimal?.SetDragable(cart == bottomCart);
        }

        isRotating = false;
    }

    // Smooth easing function for gradual acceleration and deceleration
    private float SmoothStep(float t)
    {
        // Apply cubic easing: 3t² - 2t³
        // This creates a smooth curve with gradual start and end
        return t * t * (3f - 2f * t);
    }

    // Draw gizmos when the FerrisWheel is selected in the editor
    private void OnDrawGizmosSelected()
    {
        // Draw loading zone
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(loadingZone.center, loadingZone.size);

        // Draw unloading zone
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(unloadingZone.center, unloadingZone.size);
    }
}
