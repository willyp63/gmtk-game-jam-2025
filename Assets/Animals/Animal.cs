using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : MonoBehaviour
{
    private AnimalData animalData;
    public AnimalData AnimalData => animalData;

    private int currentPoints = 0;
    public int CurrentPoints => currentPoints;

    private Cart assignedCart = null;
    public Cart AssignedCart => assignedCart;

    private FerrisWheel ferrisWheel;

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    public System.Action OnDragToLoadingZone;
    public System.Action OnDragToUnloadingZone;

    // Drag and drop variables
    private bool isDragable = true;
    private Vector3 snapPosition;
    private bool isDragging = false;
    private Camera mainCamera;
    private Collider2D collider;

    private void Awake()
    {
        mainCamera = Camera.main;
        collider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        HandleDragAndDrop();
    }

    public void UpdateSnapPosition()
    {
        snapPosition = transform.localPosition;
    }

    public void InitializeAnimal(AnimalData animalData, FerrisWheel ferrisWheel)
    {
        this.ferrisWheel = ferrisWheel;

        UpdateSnapPosition();

        if (animalData != null)
        {
            // Set the sprite
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = animalData.sprite;
            }

            // Set initial points
            currentPoints = animalData.basePoints;
        }
    }

    public void SetDragable(bool isDragable)
    {
        this.isDragable = isDragable;
    }

    public void SetPoints(int newPoints)
    {
        currentPoints = Mathf.Max(0, newPoints);
    }

    public void ResetPoints()
    {
        if (animalData != null)
        {
            currentPoints = animalData.basePoints;
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
        return collider.OverlapPoint(mousePosition);
    }

    private void StopDragging()
    {
        isDragging = false;

        Vector3 dragToPosition = transform.position;

        // Return to original position
        transform.localPosition = snapPosition;

        // Check loading zone
        if (ferrisWheel.LoadingZone.Contains(dragToPosition))
        {
            OnDragToLoadingZone?.Invoke();
        }
        // Check unloading zone
        else if (ferrisWheel.UnloadingZone.Contains(dragToPosition))
        {
            OnDragToUnloadingZone?.Invoke();
        }
    }
}
