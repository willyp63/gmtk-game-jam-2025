using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        if (modifier == AnimalModifier.Rainbow)
        {
            modifiedPoints *= 2;
        }
    }

    public string GetTooltipText()
    {
        string animalName =
            modifier != AnimalModifier.None
                ? $"{GetModifierName(modifier)} {baseAnimalData.animalName}"
                : $"{baseAnimalData.animalName}";
        return $"<size=28>{FormatNameBasedOnModifier(modifier, animalName.ToUpper())}</size>\n<size=28><color=#{ColorUtility.ToHtmlStringRGBA(FloatingTextManager.pointsColor)}>{modifiedPoints} points</color></size>\n\n<size=28>{string.Join("\n", baseAnimalData.effects.Select(e => e.tooltipText))}</size>";
    }

    public string GetTooltipTextRight()
    {
        return "";
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

    public static string GetModifierName(AnimalModifier modifier)
    {
        switch (modifier)
        {
            case AnimalModifier.Rainbow:
                return "Rainbow";
            case AnimalModifier.Fire:
                return "Fire";
            default:
                return "None";
        }
    }

    public static string FormatNameBasedOnModifier(AnimalModifier modifier, string name)
    {
        switch (modifier)
        {
            case AnimalModifier.Rainbow:
                return FormatRainbowText(name);
            case AnimalModifier.Fire:
                return $"<color=#FF4500>{name}</color>";
            default:
                return name;
        }
    }

    private static string FormatRainbowText(string text)
    {
        string[] rainbowColors =
        {
            "#FF0000",
            "#FF7F00",
            "#FFFF00",
            "#00FF00",
            "#0000FF",
            "#4B0082",
            "#9400D3",
        };
        string result = "";

        for (int i = 0; i < text.Length; i++)
        {
            int colorIndex = i % rainbowColors.Length;
            result += $"<color={rainbowColors[colorIndex]}>{text[i]}</color>";
        }

        return result;
    }
}
