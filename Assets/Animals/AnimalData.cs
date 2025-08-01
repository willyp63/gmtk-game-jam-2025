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
    OnDayEnd,
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
        Debug.Log(
            $"Applying points effect: +{effect.value1} & *{effect.value2} to animal {animal.DeckAnimal.BaseAnimalData.animalName}"
        );

        animal.AddPoints((int)effect.value1);
        animal.MultiplyPoints((int)effect.value2);
    }
}
