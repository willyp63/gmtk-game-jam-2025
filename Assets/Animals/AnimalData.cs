using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum AnimalEffectTrigger
{
    OnLoad,
    OnUnload,
    OnRotate,
    OnStop,
    OnStopTop,
    OnStopBottom,
    OnPassTop,
    OnPassBottom,
}

public enum AnimalEffectTarget
{
    Self,
    Adjacent,
    Opposite,
    All,
}

public enum AnimalEffectType
{
    Points,
}

[System.Serializable]
public class AnimalEffectData
{
    public AnimalEffectType type;
    public AnimalEffectTrigger trigger;
    public AnimalEffectTarget target;

    public float value1;
    public float value2;
    public float value3;

    [TextArea(3, 10)]
    public string tooltipText;
}

[CreateAssetMenu(fileName = "New Animal", menuName = "Ferris Wheel/Animal")]
public class AnimalData : ScriptableObject
{
    public string animalName;
    public Sprite sprite;
    public int basePoints;

    public List<AnimalEffectData> effects;

    public static void ApplyEffect(AnimalEffectData effect, Animal animal)
    {
        switch (effect.type)
        {
            case AnimalEffectType.Points:
                ApplyPointsEffect(effect, animal);
                break;
            default:
                break;
        }
    }

    private static void ApplyPointsEffect(AnimalEffectData effect, Animal animal)
    {
        int newPoints = animal.CurrentPoints;
        newPoints += (int)effect.value1;
        newPoints *= (int)effect.value2;
        animal.SetPoints(newPoints);
    }
}
