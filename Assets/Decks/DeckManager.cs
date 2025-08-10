using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

public class DeckManager : Singleton<DeckManager>
{
    [SerializeField]
    private List<DeckData> starterDecks = new();
    public List<DeckData> StarterDecks => starterDecks;

    // Player's permanent deck that persists between runs
    private List<AnimalWithModifier> permanentDeck = new();
    public List<AnimalWithModifier> PermanentDeck => new List<AnimalWithModifier>(permanentDeck);

    // Current round queue (shuffled from permanent deck)
    private Queue<DeckAnimal> currentRoundQueue = new();
    public int CurrentRoundQueueCount => currentRoundQueue.Count;

    // Events for UI updates
    public System.Action OnDeckChanged;
    public System.Action OnRoundQueueChanged;

    protected override void Awake()
    {
        base.Awake();

        if (permanentDeck.Count == 0)
        {
            SetStarterDeck(0);
        }
    }

    public void SetStarterDeck(int starterDeckIndex)
    {
        Debug.Log($"Setting starter deck to {starterDeckIndex}. Total decks: {starterDecks.Count}");
        if (starterDeckIndex >= 0 && starterDeckIndex < starterDecks.Count)
        {
            permanentDeck = new List<AnimalWithModifier>();
            permanentDeck.Clear();
            DeckData starterDeck = starterDecks[starterDeckIndex];
            Debug.Log(
                $"Cleared permanent deck. Adding  {starterDeck.animals.Count} animals from starter deck {starterDeck.deckName}"
            );
            foreach (var animal in starterDeck.animals)
            {
                permanentDeck.Add(
                    new AnimalWithModifier
                    {
                        animalData = animal.animalData,
                        modifier = animal.modifier,
                    }
                );
                Debug.Log(
                    $"Added animal {animal.animalData.animalName} with modifier {animal.modifier} to permanent deck"
                );
            }
            OnDeckChanged?.Invoke();
        }
        Debug.Log($"Permanent deck size: {permanentDeck.Count}");
    }

    public void AddAnimalToPermanentDeck(
        AnimalData animalData,
        AnimalModifier modifier = AnimalModifier.None
    )
    {
        var newAnimal = new AnimalWithModifier { animalData = animalData, modifier = modifier };

        permanentDeck.Add(newAnimal);
        OnDeckChanged?.Invoke();
    }

    public bool RemoveAnimalFromPermanentDeck(int index)
    {
        if (index >= 0 && index < permanentDeck.Count)
        {
            permanentDeck.RemoveAt(index);
            OnDeckChanged?.Invoke();
            return true;
        }
        return false;
    }

    public void GenerateNewRoundQueue()
    {
        currentRoundQueue.Clear();

        if (permanentDeck.Count == 0)
        {
            Debug.LogWarning("Cannot generate round queue: permanent deck is empty!");
            return;
        }

        // Create a shuffled list of DeckAnimals from the permanent deck
        var shuffledAnimals = new List<DeckAnimal>();
        foreach (var animalWithModifier in permanentDeck)
        {
            shuffledAnimals.Add(
                new DeckAnimal(animalWithModifier.animalData, animalWithModifier.modifier)
            );
        }

        // Shuffle the list
        for (int i = 0; i < shuffledAnimals.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffledAnimals.Count);
            var temp = shuffledAnimals[i];
            shuffledAnimals[i] = shuffledAnimals[randomIndex];
            shuffledAnimals[randomIndex] = temp;
        }

        // Add to queue
        foreach (var animal in shuffledAnimals)
        {
            currentRoundQueue.Enqueue(animal);
        }

        OnRoundQueueChanged?.Invoke();
    }

    public List<DeckAnimal> PeekNextAnimals(int count)
    {
        var result = new List<DeckAnimal>();
        var tempQueue = new Queue<DeckAnimal>(currentRoundQueue);

        for (int i = 0; i < count && tempQueue.Count > 0; i++)
        {
            result.Add(tempQueue.Dequeue());
        }

        return result;
    }

    public List<DeckAnimal> GetNextAnimals(int count)
    {
        var result = new List<DeckAnimal>();

        for (int i = 0; i < count && currentRoundQueue.Count > 0; i++)
        {
            result.Add(currentRoundQueue.Dequeue());
        }

        OnRoundQueueChanged?.Invoke();
        return result;
    }

    public DeckAnimal GetNextAnimal()
    {
        if (currentRoundQueue.Count > 0)
        {
            var animal = currentRoundQueue.Dequeue();
            OnRoundQueueChanged?.Invoke();
            return animal;
        }
        return null;
    }

    public void AddAnimalToBackOfQueue(DeckAnimal animal)
    {
        currentRoundQueue.Enqueue(animal);
        OnRoundQueueChanged?.Invoke();
    }
}
