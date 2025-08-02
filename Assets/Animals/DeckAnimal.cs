using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public enum AnimalModifier
{
    None,
    Rainbow,
    Negative,
    Fire,
    Lightning,
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
        this.modifiedEffects = new List<AnimalEffectData>(
            animalData.effects.Select(e => new AnimalEffectData(e))
        );

        ApplyModifierEffects();
    }

    public string GetTooltipText()
    {
        string nameWithModifier =
            modifier != AnimalModifier.None
                ? $"{GetModifierName(modifier)} {baseAnimalData.animalName}"
                : $"{baseAnimalData.animalName}";

        string nameText =
            $"<size=32>{FormatNameBasedOnModifier(modifier, nameWithModifier.ToUpper())}</size>";

        string pointsText =
            $"<size=28><color=#{ColorUtility.ToHtmlStringRGBA(FloatingTextManager.pointsColor)}>{modifiedPoints} points</color></size>";

        string effectsText =
            $"<size=28>{string.Join("\n", modifiedEffects.Select(e => FormatTooltipText(e.tooltipText, e)).Where(e => e != ""))}</size>";

        return $"{nameText}\n{pointsText}\n\n{effectsText}";
    }

    private string FormatTooltipText(string text, AnimalEffectData effect)
    {
        string pattern = @"#VAL#";

        if (effect.type == AnimalEffectType.AddPoints)
        {
            string signText = effect.value1 > 0 ? "+" : "";
            return Regex.Replace(text, pattern, $"{signText}{effect.value1}");
        }
        else if (effect.type == AnimalEffectType.MultiplyPoints)
        {
            return Regex.Replace(text, pattern, $"x{effect.value1}");
        }
        else if (effect.type == AnimalEffectType.SpinWheel)
        {
            return Regex.Replace(text, pattern, Mathf.Abs(effect.value1).ToString());
        }
        else
        {
            return Regex.Replace(text, pattern, effect.value1.ToString());
        }
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
            case AnimalModifier.Negative:
                return "Opposite";
            case AnimalModifier.Fire:
                return "Fire";
            case AnimalModifier.Lightning:
                return "Hologram";
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
            case AnimalModifier.Negative:
                return $"<color=#333333>{name}</color>";
            case AnimalModifier.Fire:
                return $"<color=#FF4500>{name}</color>";
            case AnimalModifier.Lightning:
                return $"<color=#00FFFF>{name}</color>";
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
            case AnimalModifier.Negative:
                ApplyNegativeEffect();
                break;
            case AnimalModifier.Fire:
                ApplyFireEffect();
                break;
            case AnimalModifier.Lightning:
                ApplyLightningEffect();
                break;
            default:
                break;
        }
    }

    private void ApplyLightningEffect()
    {
        modifiedPoints = 0;
        foreach (AnimalEffectData effect in modifiedEffects)
        {
            effect.trigger = AnimalEffectTrigger.OnRotate;

            effect.tooltipText = Regex.Replace(
                effect.tooltipText,
                @"<color=#00ffff>(.*?)</color>",
                "<color=#00ffff>ROTATE:</color>"
            );
        }
    }

    private void ApplyFireEffect()
    {
        modifiedPoints = 0;
        foreach (AnimalEffectData effect in modifiedEffects)
        {
            effect.value1 *= 2;
        }
    }

    private void ApplyNegativeEffect()
    {
        if (modifiedEffects[0] != null)
        {
            modifiedEffects[0].tooltipText = modifiedEffects[0].tooltipTextOpposite;
            if (modifiedEffects[0].type == AnimalEffectType.SpinWheel)
            {
                modifiedEffects[0].value1 *= -1;
            }
            else
            {
                modifiedEffects[0].target = AnimalEffectTarget.Opposite;
            }
        }
    }
}
