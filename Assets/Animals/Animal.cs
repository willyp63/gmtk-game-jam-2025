using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Animal : MonoBehaviour
{
    private DeckAnimal deckAnimal;
    public DeckAnimal DeckAnimal => deckAnimal;

    private int currentPoints = 0;
    public int CurrentPoints => currentPoints;

    private Cart assignedCart = null;
    public Cart AssignedCart => assignedCart;

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private TooltipTrigger tooltipTrigger;

    [SerializeField]
    private Material defaultMaterial;

    [SerializeField]
    private Material rainbowMaterial;

    [SerializeField]
    private Material fireMaterial;

    public System.Action<Vector3> OnDragToLoadingZone;
    public System.Action<Vector3> OnDragToSkipZone;

    // Drag and drop variables
    private bool isDragable = true;
    private Vector3 snapPosition;
    private bool isDragging = false;
    private Camera mainCamera;
    private Collider2D dragCollider;
    private Animator animator;

    private void Awake()
    {
        mainCamera = Camera.main;
        dragCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        HandleDragAndDrop();
    }

    public void ApplyEffects(AnimalEffectTrigger trigger)
    {
        if (deckAnimal == null)
            return;

        Debug.Log($"Applying effects for {trigger} on {deckAnimal.BaseAnimalData.animalName}");

        foreach (AnimalEffectData effect in deckAnimal.ModifiedEffects)
        {
            if (effect.trigger == trigger)
            {
                List<Animal> targets = GetEffectTargets(effect)
                    .Where(animal => animal != null)
                    .ToList();
                foreach (Animal target in targets)
                {
                    AnimalData.ApplyEffect(effect, target);
                }
            }
        }
    }

    private List<Animal> GetEffectTargets(AnimalEffectData effect)
    {
        switch (effect.target)
        {
            case AnimalEffectTarget.Self:
                return new List<Animal> { this };
            case AnimalEffectTarget.Adjacent:
                return new List<Animal>
                {
                    FerrisWheel.Instance.GetAdjacentCart(AssignedCart, true).CurrentAnimal,
                    FerrisWheel.Instance.GetAdjacentCart(AssignedCart, false).CurrentAnimal,
                };
            case AnimalEffectTarget.Opposite:
                return new List<Animal>
                {
                    FerrisWheel.Instance.GetOppositeCart(AssignedCart).CurrentAnimal,
                };
            case AnimalEffectTarget.All:
                return FerrisWheel.Instance.Carts.Select(cart => cart.CurrentAnimal).ToList();
            default:
                return new List<Animal>();
        }
    }

    public void SetIsMoving(bool isMoving)
    {
        animator.SetBool("IsMoving", isMoving);
    }

    public void UpdateSnapPosition()
    {
        snapPosition = transform.localPosition;
    }

    public void InitializeAnimal(DeckAnimal deckAnimal)
    {
        this.deckAnimal = deckAnimal;

        UpdateSnapPosition();

        // Set the sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = deckAnimal.BaseAnimalData.sprite;
            switch (deckAnimal.Modifier)
            {
                case AnimalModifier.Rainbow:
                    spriteRenderer.material = rainbowMaterial;
                    break;
                case AnimalModifier.Fire:
                    spriteRenderer.material = fireMaterial;
                    break;
                default:
                    spriteRenderer.material = defaultMaterial;
                    break;
            }
        }

        // Set initial points
        currentPoints = deckAnimal.ModifiedPoints;

        // Set tooltip text
        if (tooltipTrigger != null)
        {
            tooltipTrigger.SetTooltipText(
                deckAnimal.GetTooltipText(),
                deckAnimal.GetTooltipTextRight()
            );
        }
    }

    public void SetDragable(bool isDragable)
    {
        this.isDragable = isDragable;
    }

    public void AddPoints(int points)
    {
        currentPoints += points;
    }

    public void MultiplyPoints(int multiplier)
    {
        currentPoints *= multiplier;
    }

    public void ResetPoints()
    {
        if (deckAnimal != null)
        {
            currentPoints = deckAnimal.ModifiedPoints;
        }
        else
        {
            currentPoints = 0;
        }
    }

    public void AssignToCart(Cart cart)
    {
        assignedCart = cart;
    }

    public void RemoveFromCart()
    {
        assignedCart = null;
    }

    private void HandleDragAndDrop()
    {
        if (!isDragable)
            return;

        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        // Check if mouse is over this animal
        bool mouseOverAnimal = IsMouseOverAnimal(mousePosition);

        // Start dragging
        if (Input.GetMouseButtonDown(0) && mouseOverAnimal && !isDragging)
        {
            isDragging = true;
        }

        // Continue dragging
        if (isDragging)
        {
            // Update position to follow mouse
            transform.position = mousePosition;

            // Stop dragging
            if (Input.GetMouseButtonUp(0))
            {
                StopDragging();
            }
        }
    }

    private bool IsMouseOverAnimal(Vector3 mousePosition)
    {
        return dragCollider.OverlapPoint(mousePosition);
    }

    private void StopDragging()
    {
        isDragging = false;

        Vector3 dragToPosition = transform.position;

        // Return to original position
        transform.localPosition = snapPosition;

        // Check loading zone
        if (FerrisWheel.Instance.LoadingZone.Contains(dragToPosition))
        {
            OnDragToLoadingZone?.Invoke(dragToPosition);
        }
        else if (FerrisWheel.Instance.SkipZone.Contains(dragToPosition))
        {
            OnDragToSkipZone?.Invoke(dragToPosition);
        }
    }
}
