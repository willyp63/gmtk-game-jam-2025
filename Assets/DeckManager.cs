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
public class TestAnimal
{
    public AnimalData animalData;
    public AnimalModifier modifier;
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
    private List<TestAnimal> testAnimals = new();

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

    // Queue to store animals in order of priority
    private Queue<DeckAnimal> animalQueue = new Queue<DeckAnimal>();
    private bool isInitialized = false;

    private void InitializeQueue()
    {
        AnimalModifier[] modifiers = new AnimalModifier[]
        {
            AnimalModifier.None,
            AnimalModifier.Rainbow,
            AnimalModifier.Negative,
            AnimalModifier.Fire,
            AnimalModifier.Lightning,
        };

        foreach (var modifier in modifiers)
        {
            foreach (var testAnimal in testAnimals)
            {
                if (testAnimal.animalData != null)
                {
                    DeckAnimal deckAnimal = new DeckAnimal(testAnimal.animalData, modifier);
                    animalQueue.Enqueue(deckAnimal);
                }
            }
        }

        isInitialized = true;
    }

    public List<DeckAnimal> DequeueAnimals(int count)
    {
        if (!isInitialized)
            InitializeQueue();

        List<DeckAnimal> result = new List<DeckAnimal>();

        for (int i = 0; i < count; i++)
        {
            // If queue is empty, generate a random animal
            if (animalQueue.Count == 0)
            {
                DeckAnimal randomAnimal = GenerateRandomAnimal();
                if (randomAnimal != null)
                {
                    result.Add(randomAnimal);
                }
            }
            else
            {
                // Dequeue the next animal from the queue
                result.Add(animalQueue.Dequeue());
            }
        }

        return result;
    }

    private DeckAnimal GenerateRandomAnimal()
    {
        // Select random animal data based on rarity weights
        AnimalData selectedAnimalData = GetRandomAnimalData();

        if (selectedAnimalData == null)
        {
            return null;
        }

        // Select random modifier based on rarity weights (with chance of no modifier)
        AnimalModifier selectedModifier = GetRandomModifier();

        // Create DeckAnimal with selected data and modifier
        return new DeckAnimal(selectedAnimalData, selectedModifier);
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
