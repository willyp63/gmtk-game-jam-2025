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

    private List<AnimalEffectData> modifiedEffects;
    public List<AnimalEffectData> ModifiedEffects => modifiedEffects;

    public DeckAnimal(AnimalData animalData, AnimalModifier modifier = AnimalModifier.None)
    {
        this.baseAnimalData = animalData;
        this.modifier = modifier;
        this.modifiedPoints = animalData.basePoints;
        this.modifiedEffects = new List<AnimalEffectData>(animalData.effects);

        ApplyModifierEffects();
    }

    public string GetTooltipText()
    {
        string nameWithModifier =
            modifier != AnimalModifier.None
                ? $"{GetModifierName(modifier)} {baseAnimalData.animalName}"
                : $"{baseAnimalData.animalName}";

        string nameText =
            $"<size=28>{FormatNameBasedOnModifier(modifier, nameWithModifier.ToUpper())}</size>";

        string pointsText =
            $"<size=28><color=#{ColorUtility.ToHtmlStringRGBA(FloatingTextManager.pointsColor)}>{modifiedPoints} points</color></size>";

        string effectsText =
            $"<size=28>{string.Join("\n", modifiedEffects.Select(e => e.tooltipText))}</size>";

        return $"{nameText}\n{pointsText}\n\n{effectsText}";
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

    private void ApplyModifierEffects()
    {
        switch (modifier)
        {
            case AnimalModifier.Rainbow:
                modifiedPoints *= 2;
                break;
            case AnimalModifier.Fire:
                ApplyFireEffect();
                break;
            default:
                break;
        }
    }

    private void ApplyFireEffect()
    {
        AnimalEffectData fireEffect = new()
        {
            type = AnimalEffectType.Points,
            trigger = AnimalEffectTrigger.OnRotate,
            target = AnimalEffectTarget.Adjacent,
            value1 = -5,
            value2 = 1,
            value3 = 0,
            tooltipText = "<color=#00ffff>ON ROTATE:</color> -5 points to adjacent animals",
        };

        modifiedEffects.Add(fireEffect);
    }
}
