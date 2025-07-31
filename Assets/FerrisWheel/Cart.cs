using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cart : MonoBehaviour
{
    [SerializeField]
    private Transform hinge;
    public Transform Hinge => hinge;

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private Sprite closedSprite;

    [SerializeField]
    private Sprite openSprite;

    private Animal currentAnimal;
    public Animal CurrentAnimal => currentAnimal;

    public bool IsEmpty => currentAnimal == null;

    public void LoadAnimal(Animal animal)
    {
        currentAnimal = animal;
        currentAnimal.transform.parent = transform;
        currentAnimal.transform.localPosition = Vector3.zero;

        animal.AssignToCart(this);
        animal.UpdateSnapPosition();
    }

    public void UnloadAnimal()
    {
        currentAnimal.RemoveFromCart();
        currentAnimal.transform.parent = null;
        currentAnimal = null;
    }

    public void SetOpen(bool isOpen)
    {
        if (isOpen)
        {
            spriteRenderer.sprite = openSprite;
        }
        else
        {
            spriteRenderer.sprite = closedSprite;
        }
    }
}
