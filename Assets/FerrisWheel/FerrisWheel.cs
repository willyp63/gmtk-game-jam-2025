using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FerrisWheel : MonoBehaviour
{
    [SerializeField]
    private List<Cart> carts = new(); // Cart #1 is initially at the top. Carts are in clockwise order.
    public List<Cart> Carts => carts;

    [SerializeField]
    private Bounds loadingZone;
    public Bounds LoadingZone => loadingZone;

    [SerializeField]
    private Transform unloadingLocation;
    public Transform UnloadingLocation => unloadingLocation;

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

            animal.SetDragable(false);
            animal.ApplyEffects(AnimalEffectTrigger.OnLoad);
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

    public void RotateWheel(bool isClockwise, int steps)
    {
        if (isRotating)
            return;

        if (!RoundManager.Instance.ConsumeEnergy(steps))
            return;

        StartCoroutine(Rotate(isClockwise, steps));
    }

    private IEnumerator Rotate(bool isClockwise, int steps)
    {
        isRotating = true;

        foreach (Cart cart in carts)
        {
            cart.SetOpen(false);
        }

        float angle = (isClockwise ? -rotationAngle : rotationAngle) * steps;
        Quaternion targetRotation = transform.rotation * Quaternion.Euler(0, 0, angle);

        Quaternion startRotation = transform.rotation;
        float adjustedRotationSpeed = rotationSpeed * (1 + (steps - 1) * 0.15f);
        float elapsedTime = 0f;
        float duration = Mathf.Abs(angle) / adjustedRotationSpeed;
        int currentStep = 1;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Apply smooth easing: slow start, fast middle, slow end
            float easedT = SmoothStep(t);

            // Use smooth interpolation for the rotation
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, easedT);

            // After each rotationAngle, apply OnRotate to all animals
            float deltaAngle = Mathf.Abs(
                transform.rotation.eulerAngles.z - startRotation.eulerAngles.z
            );
            if (currentStep < steps && deltaAngle >= rotationAngle * currentStep)
            {
                topCartIndex = (topCartIndex + (isClockwise ? -1 : 1) + carts.Count) % carts.Count;
                currentStep++;

                foreach (Cart cart in carts)
                {
                    cart.CurrentAnimal?.ApplyEffects(AnimalEffectTrigger.OnRotate);
                }
                GetTopCart().CurrentAnimal?.ApplyEffects(AnimalEffectTrigger.OnPassTop);
                GetBottomCart().CurrentAnimal?.ApplyEffects(AnimalEffectTrigger.OnPassBottom);
            }

            yield return null;
        }

        // Ensure we end up exactly at the target rotation
        transform.rotation = targetRotation;

        // Update the top cart index
        topCartIndex = (topCartIndex + (isClockwise ? -1 : 1) + carts.Count) % carts.Count;

        foreach (Cart cart in carts)
        {
            cart.CurrentAnimal?.ApplyEffects(AnimalEffectTrigger.OnRotate);
        }
        GetTopCart().CurrentAnimal?.ApplyEffects(AnimalEffectTrigger.OnPassTop);
        GetBottomCart().CurrentAnimal?.ApplyEffects(AnimalEffectTrigger.OnPassBottom);

        foreach (Cart cart in carts)
        {
            cart.CurrentAnimal?.ApplyEffects(AnimalEffectTrigger.OnStop);
        }
        GetTopCart().CurrentAnimal?.ApplyEffects(AnimalEffectTrigger.OnStopTop);
        GetBottomCart().CurrentAnimal?.ApplyEffects(AnimalEffectTrigger.OnStopBottom);

        // Unload animal in bottom cart
        Cart bottomCart = GetBottomCart();
        bottomCart.SetOpen(true);
        if (!bottomCart.IsEmpty)
        {
            Animal animal = bottomCart.CurrentAnimal;
            animal.ApplyEffects(AnimalEffectTrigger.OnUnload);

            bottomCart.UnloadAnimal();

            // Add animal back to queue
            DeckManager.Instance.EnqueueAnimal(animal.AnimalData);

            RoundManager.Instance.AddScore(animal.CurrentPoints);

            FloatingTextManager.Instance.SpawnText(
                $"+{animal.CurrentPoints}",
                animal.transform.position,
                FloatingTextManager.pointsColor,
                1.5f
            );

            // Move the animal to the unloading location, then animate it moving to the right 10 units, then destroy it
            StartCoroutine(AnimateAnimalUnloading(animal));
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

    private IEnumerator AnimateAnimalUnloading(Animal animal)
    {
        if (animal == null || unloadingLocation == null)
        {
            Destroy(animal?.gameObject);
            yield break;
        }

        animal.SetIsMoving(true);

        // Ensure animal is exactly at unloading position
        animal.transform.position = unloadingLocation.position;
        animal.transform.rotation = Quaternion.identity;

        // Animate animal moving to the right 12 units
        Vector3 finalPosition = unloadingLocation.position + Vector3.right * 12f;
        float moveRightDuration = 2.0f;
        float elapsedTime = 0f;

        while (elapsedTime < moveRightDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveRightDuration;
            float easedT = SmoothStep(t);

            animal.transform.position = Vector3.Lerp(
                unloadingLocation.position,
                finalPosition,
                easedT
            );
            yield return null;
        }

        // Destroy the animal
        Destroy(animal.gameObject);
    }

    // Draw gizmos when the FerrisWheel is selected in the editor
    private void OnDrawGizmosSelected()
    {
        // Draw loading zone
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(loadingZone.center, loadingZone.size);
    }
}
