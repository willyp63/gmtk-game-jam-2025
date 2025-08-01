using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
}

[System.Serializable]
public class AnimalOption
{
    public AnimalData animalData;
    public Rarity rarity;
}

[System.Serializable]
public class ModifierOption
{
    public AnimalModifier modifier;
    public float weight;
}

public class DeckManager : Singleton<DeckManager>
{
    [SerializeField]
    private List<AnimalOption> animalOptions = new();

    [SerializeField]
    private List<ModifierOption> modifierOptions = new();

    [SerializeField]
    private float commonWeight = 4;

    [SerializeField]
    private float uncommonWeight = 2;

    [SerializeField]
    private float rareWeight = 1;

    public List<DeckAnimal> DequeueAnimals(int count)
    {
        List<DeckAnimal> result = new List<DeckAnimal>();

        for (int i = 0; i < count; i++)
        {
            // Select random animal data based on rarity weights
            AnimalData selectedAnimalData = GetRandomAnimalData();

            // Select random modifier based on rarity weights (with chance of no modifier)
            AnimalModifier selectedModifier = GetRandomModifier();

            // Create DeckAnimal with selected data and modifier
            DeckAnimal newAnimal = new DeckAnimal(selectedAnimalData, selectedModifier);
            result.Add(newAnimal);
        }

        return result;
    }

    private AnimalData GetRandomAnimalData()
    {
        if (animalOptions.Count == 0)
        {
            Debug.LogWarning("No animal options available!");
            return null;
        }

        // Calculate total weight
        float totalWeight = 0f;
        foreach (var option in animalOptions)
        {
            totalWeight += GetWeightForRarity(option.rarity);
        }

        if (totalWeight <= 0f)
        {
            Debug.LogWarning("Total weight is 0 or negative!");
            return null;
        }

        // Generate random value
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        // Find the option that corresponds to the random value
        foreach (var option in animalOptions)
        {
            currentWeight += GetWeightForRarity(option.rarity);
            if (randomValue < currentWeight)
            {
                return option.animalData;
            }
        }

        // Fallback (shouldn't reach here if weights are positive)
        return animalOptions[animalOptions.Count - 1].animalData;
    }

    private AnimalModifier GetRandomModifier()
    {
        // Calculate total weight
        float totalWeight = 0f;

        foreach (var option in modifierOptions)
        {
            totalWeight += option.weight;
        }

        if (totalWeight <= 0f)
        {
            return AnimalModifier.None;
        }

        // Generate random value
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        // Check modifier options first
        foreach (var option in modifierOptions)
        {
            currentWeight += option.weight;
            if (randomValue < currentWeight)
            {
                return option.modifier;
            }
        }

        // If we haven't selected a modifier yet, return "None"
        return AnimalModifier.None;
    }

    private float GetWeightForRarity(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                return commonWeight;
            case Rarity.Uncommon:
                return uncommonWeight;
            case Rarity.Rare:
                return rareWeight;
            default:
                return 1f;
        }
    }
}
