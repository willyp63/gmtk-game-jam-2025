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
    Random,
}

public enum AnimalEffectType
{
    AddPoints,
    MultiplyPoints,
    CopyPoints,
    SpinWheel,
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

    [TextArea(3, 10)]
    public string tooltipTextOpposite;

    public AnimalEffectData(AnimalEffectData other)
    {
        type = other.type;
        trigger = other.trigger;
        target = other.target;
        value1 = other.value1;
        value2 = other.value2;
        value3 = other.value3;
        tooltipText = other.tooltipText;
        tooltipTextOpposite = other.tooltipTextOpposite;
    }
}

[CreateAssetMenu(fileName = "New Animal", menuName = "Ferris Wheel/Animal")]
public class AnimalData : ScriptableObject
{
    public string animalName;
    public Sprite sprite;
    public int basePoints;

    public List<AnimalEffectData> effects;

    public static void ApplyEffect(AnimalEffectData effect, Animal animal, Animal source)
    {
        switch (effect.type)
        {
            case AnimalEffectType.AddPoints:
                animal.AddPoints((int)effect.value1);
                break;
            case AnimalEffectType.MultiplyPoints:
                animal.MultiplyPoints((int)effect.value1);
                break;
            case AnimalEffectType.CopyPoints:
                source.SetPoints(animal.CurrentPoints);
                break;
            case AnimalEffectType.SpinWheel:
                GameManager.Instance.FerrisWheel.RotateWheel(
                    effect.value1 < 0,
                    (int)Mathf.Abs(effect.value1)
                );
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
