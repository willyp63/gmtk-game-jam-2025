using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Animal", menuName = "Ferris Wheel/Animal")]
public class AnimalData : ScriptableObject
{
    public string animalName;
    public Sprite sprite;
    public int loadingCost;
    public int basePoints;
}
