using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FerrisWheel : Singleton<FerrisWheel>
{
    [SerializeField]
    private Cart cartPrefab;

    [SerializeField]
    private GameObject cartContainer;

    [SerializeField]
    private GameObject hingePrefab;

    [Header("Dynamic Cart Configuration")]
    [SerializeField]
    private int numberOfCarts = 8; // Must be even and > 2

    [SerializeField]
    private float cartDistanceFromCenter = 3f; // Distance from wheel center to cart hinges

    [SerializeField]
    private Sprite wheelSpriteX6;

    [SerializeField]
    private Sprite wheelSpriteX8;

    [SerializeField]
    private SpriteRenderer wheelSpriteRenderer;

    public int NumberOfCarts => numberOfCarts;
    public float CartDistanceFromCenter => cartDistanceFromCenter;

    private List<Cart> carts = new(); // Cart #1 is initially at the top. Carts are in clockwise order.
    public List<Cart> Carts => carts;

    [SerializeField]
    private Bounds skipZone;
    public Bounds SkipZone => skipZone;

    [SerializeField]
    private Bounds loadingZone;
    public Bounds LoadingZone => loadingZone;

    [SerializeField]
    private Transform unloadingLocation;
    public Transform UnloadingLocation => unloadingLocation;

    [SerializeField]
    private float rotationSpeed = 90f; // degrees per second

    private float rotationAngle = 45f;

    private int topCartIndex = 0;
    private bool isRotating = false;

    // Events
    public System.Action OnWheelStopped;

    public void Initialize()
    {
        // Validate cart count
        if (numberOfCarts < 4 || numberOfCarts % 2 != 0)
        {
            Debug.LogError(
                $"FerrisWheel: numberOfCarts must be even and >= 4. Current value: {numberOfCarts}"
            );
            numberOfCarts = Mathf.Max(4, numberOfCarts + (numberOfCarts % 2));
        }

        // Calculate angle between carts
        rotationAngle = 360f / numberOfCarts;

        if (numberOfCarts % 3 == 0)
        {
            wheelSpriteRenderer.sprite = wheelSpriteX6;
        }
        else
        {
            wheelSpriteRenderer.sprite = wheelSpriteX8;
        }

        // Create carts and hinges dynamically
        CreateCartsAndHinges();
    }

    private void CreateCartsAndHinges()
    {
        for (int i = 0; i < numberOfCarts; i++)
        {
            // Calculate position for this cart
            // Start at top (270 degrees) and go clockwise
            float angle = 360f - (i * rotationAngle);
            Vector3 hingePosition =
                transform.position
                + Quaternion.Euler(0, 0, angle) * Vector3.up * cartDistanceFromCenter;

            // Create hinge
            GameObject hinge = Instantiate(
                hingePrefab,
                hingePosition,
                Quaternion.identity,
                transform
            );
            hinge.name = $"Hinge_{i + 1}";

            // Create cart
            Cart cart = Instantiate(
                cartPrefab,
                Vector3.zero,
                Quaternion.identity,
                cartContainer.transform
            );
            cart.name = $"Cart_{i + 1}";

            // Initialize cart with hinge
            cart.Initialize(hinge);

            // Add to carts list
            carts.Add(cart);
        }

        GetBottomCart().SetOpen(true);
    }

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

    public Cart GetAdjacentCart(Cart cart, bool isClockwise)
    {
        int cartIndex = carts.IndexOf(cart);
        int index = (cartIndex + (isClockwise ? 1 : -1) + carts.Count) % carts.Count;
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
        float targetAngle = transform.rotation.eulerAngles.z + angle;

        float startAngle = transform.rotation.eulerAngles.z;
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
            transform.rotation = Quaternion.Euler(
                0,
                0,
                Mathf.Lerp(startAngle, targetAngle, easedT)
            );

            // After each rotationAngle, apply OnRotate to all animals
            float deltaAngle = Mathf.Abs(transform.rotation.eulerAngles.z - startAngle);
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
        transform.rotation = Quaternion.Euler(0, 0, targetAngle);

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

            RoundManager.Instance.AddScore(animal.CurrentPoints);

            FloatingTextManager.Instance.SpawnText(
                animal.CurrentPoints > 0 ? $"+{animal.CurrentPoints}" : $"{animal.CurrentPoints}",
                animal.transform.position,
                FloatingTextManager.pointsColor,
                1.5f
            );

            // Move the animal to the unloading location, then animate it moving to the right 10 units, then destroy it
            StartCoroutine(AnimateAnimalUnloading(animal));
        }

        // If the round is over, apply the OnDayEnd effect to all animals and subtract points
        if (RoundManager.Instance.CurrentEnergy <= 0)
        {
            SubtractPointsFromAnimals();
        }

        isRotating = false;
        OnWheelStopped?.Invoke();
    }

    public void EndDayEarly()
    {
        RoundManager.Instance.ConsumeEnergy(RoundManager.Instance.CurrentEnergy);

        SubtractPointsFromAnimals();

        OnWheelStopped?.Invoke();
    }

    private void SubtractPointsFromAnimals()
    {
        foreach (Cart cart in carts)
        {
            if (!cart.IsEmpty)
            {
                Animal animal = cart.CurrentAnimal;
                animal.ApplyEffects(AnimalEffectTrigger.OnDayEnd);

                int negativePoints = -animal.CurrentPoints;
                RoundManager.Instance.AddScore(negativePoints);

                FloatingTextManager.Instance.SpawnText(
                    negativePoints > 0 ? $"+{negativePoints}" : $"{negativePoints}",
                    animal.transform.position,
                    FloatingTextManager.pointsColor,
                    1.5f
                );
            }
        }
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

        // Draw skip zone
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(skipZone.center, skipZone.size);
    }

    public void ClearWheel()
    {
        // Unload all carts and destroy animals
        foreach (Cart cart in carts)
        {
            if (!cart.IsEmpty)
            {
                Animal animal = cart.CurrentAnimal;
                cart.UnloadAnimal();

                // Destroy the animal immediately
                if (animal != null)
                {
                    Destroy(animal.gameObject);
                }
            }
        }
    }
}
