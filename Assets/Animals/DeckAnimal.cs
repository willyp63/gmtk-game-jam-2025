using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimalModifier
{
    None,
    Rainbow,
    Fire,
}

public class DeckAnimal
{
    private AnimalData baseAnimalData;
    public AnimalData BaseAnimalData => baseAnimalData;

    private AnimalModifier modifier = AnimalModifier.None;
    public AnimalModifier Modifier => modifier;

    private int modifiedPoints;
    public int ModifiedPoints => modifiedPoints;

    public DeckAnimal(AnimalData animalData, AnimalModifier modifier = AnimalModifier.None)
    {
        this.baseAnimalData = animalData;
        this.modifier = modifier;
        this.modifiedPoints = animalData != null ? animalData.basePoints : 0;
    }

    public void SetModifier(AnimalModifier newModifier)
    {
        modifier = newModifier;
    }

    public void SetModifiedPoints(int points)
    {
        modifiedPoints = Mathf.Max(0, points);
    }

    public void ResetToBasePoints()
    {
        if (baseAnimalData != null)
        {
            modifiedPoints = baseAnimalData.basePoints;
        }
    }
}
