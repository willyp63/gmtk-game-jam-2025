using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    GameObject chicken;

    [SerializeField]
    List<Material> materials;

    private int selectedIndex = 0;

    void Start()
    {
        SetMaterial();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedIndex++;
            if (selectedIndex >= materials.Count)
            {
                selectedIndex = 0;
            }
            SetMaterial();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selectedIndex--;
            if (selectedIndex < 0)
            {
                selectedIndex = materials.Count - 1;
            }
            SetMaterial();
        }
    }

    private void SetMaterial()
    {
        chicken.GetComponent<SpriteRenderer>().material = materials[selectedIndex];
    }
}
