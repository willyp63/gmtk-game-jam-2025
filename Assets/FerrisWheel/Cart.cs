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
    private SpriteRenderer backSpriteRenderer;

    [SerializeField]
    private Sprite closedSprite;

    [SerializeField]
    private Sprite openSprite;

    private Animal currentAnimal;
    public Animal CurrentAnimal => currentAnimal;

    public bool IsEmpty => currentAnimal == null;

    public void Initialize(GameObject hinge)
    {
        this.hinge = hinge.transform;
        HingeJoint2D hingeJoint = GetComponent<HingeJoint2D>();
        hingeJoint.connectedBody = hinge.GetComponent<Rigidbody2D>();

        Vector2 originalAnchor = hingeJoint.anchor;
        Vector2 originalHingePosition = hinge.transform.localPosition;

        hingeJoint.anchor = Vector2.zero;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        hinge.transform.localPosition = originalHingePosition - originalAnchor;

        // wait for 1 frame and then set the anchor back
        StartCoroutine(SetAnchorBack(originalAnchor, originalHingePosition));
    }

    private IEnumerator SetAnchorBack(Vector2 originalAnchor, Vector2 originalHingePosition)
    {
        yield return null;
        HingeJoint2D hingeJoint = GetComponent<HingeJoint2D>();
        hingeJoint.anchor = originalAnchor;
        hinge.transform.localPosition = originalHingePosition;
    }

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
