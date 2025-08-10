using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimalManager : Singleton<AnimalManager>
{
    [SerializeField]
    private List<AnimalOption> animalOptions = new();

    [SerializeField]
    private List<AnimalWithModifier> invalidAnimals = new();

    [SerializeField]
    private float commonWeight = 4;

    [SerializeField]
    private float uncommonWeight = 2;

    [SerializeField]
    private float rareWeight = 1;

    public List<DeckAnimal> GetRandomAnimals(
        int count,
        float modifierChance,
        List<Rarity> allowedRarities = null
    )
    {
        var result = new List<DeckAnimal>();

        for (int i = 0; i < count; i++)
        {
            result.Add(GenerateRandomAnimal(modifierChance, allowedRarities));
        }

        return result;
    }

    private DeckAnimal GenerateRandomAnimal(float modifierChance, List<Rarity> allowedRarities)
    {
        const int maxRetries = 100;

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            // Select random animal data based on rarity weights
            AnimalData selectedAnimalData = GetRandomAnimalData(allowedRarities);

            if (selectedAnimalData == null)
            {
                Debug.LogWarning(
                    $"Attempt {attempt + 1}: Failed to get random animal data, retrying..."
                );
                continue;
            }

            // Select random modifier based on rarity weights (with chance of no modifier)
            AnimalModifier selectedModifier = GetRandomModifier(modifierChance);

            if (
                invalidAnimals.Any(a =>
                    a.animalData.animalName == selectedAnimalData.animalName
                    && a.modifier == selectedModifier
                )
            )
            {
                Debug.LogWarning($"Attempt {attempt + 1}: Generated invalid animal, retrying...");
                continue;
            }

            return new DeckAnimal(selectedAnimalData, selectedModifier);
        }

        Debug.LogError($"Failed to generate valid animal after {maxRetries} attempts!");
        return null;
    }

    private AnimalData GetRandomAnimalData(List<Rarity> allowedRarities)
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
            if (allowedRarities == null || allowedRarities.Contains(option.rarity))
            {
                totalWeight += GetWeightForRarity(option.rarity);
            }
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

    private AnimalModifier GetRandomModifier(float modifierChance)
    {
        AnimalModifier[] modifiers = new AnimalModifier[]
        {
            AnimalModifier.Rainbow,
            AnimalModifier.Negative,
            AnimalModifier.Fire,
            AnimalModifier.Lightning,
        };

        if (Random.Range(0f, 1f) < modifierChance)
        {
            return modifiers[Random.Range(0, modifiers.Length)];
        }

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
