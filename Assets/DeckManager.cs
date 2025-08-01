using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class InitalDeckAnimal
{
    public AnimalData animalData;
    public AnimalModifier modifier;
    public int count;
}

public class DeckManager : Singleton<DeckManager>
{
    [SerializeField]
    private List<InitalDeckAnimal> initialDeck = new();

    // Player's full deck of animals
    private List<DeckAnimal> playerDeck = new();

    // Daily queue of animals (shuffled from deck)
    private Queue<DeckAnimal> animalQueue = new();

    // Events
    public System.Action OnDeckChanged;
    public System.Action OnQueueRegenerated;

    public void InitializeDeck()
    {
        playerDeck.Clear();

        foreach (InitalDeckAnimal initialDeckAnimal in initialDeck)
        {
            for (int i = 0; i < initialDeckAnimal.count; i++)
            {
                DeckAnimal deckAnimal = new DeckAnimal(
                    initialDeckAnimal.animalData,
                    initialDeckAnimal.modifier
                );
                playerDeck.Add(deckAnimal);
            }
        }

        OnDeckChanged?.Invoke();
    }

    public void RegenerateQueue()
    {
        animalQueue.Clear();

        // Create a shuffled list from the deck
        List<DeckAnimal> shuffledDeck = new List<DeckAnimal>(playerDeck);
        ShuffleList(shuffledDeck);

        // Fill the queue up
        for (int i = 0; i < shuffledDeck.Count; i++)
        {
            animalQueue.Enqueue(shuffledDeck[i]);
        }

        OnQueueRegenerated?.Invoke();
    }

    public List<DeckAnimal> DequeueAnimals(int count)
    {
        List<DeckAnimal> result = new List<DeckAnimal>();

        for (int i = 0; i < count && animalQueue.Count > 0; i++)
        {
            result.Add(animalQueue.Dequeue());
        }

        return result;
    }

    public void EnqueueAnimal(DeckAnimal animal)
    {
        animalQueue.Enqueue(animal);
    }

    public void AddAnimalToDeck(DeckAnimal animal)
    {
        playerDeck.Add(animal);
        OnDeckChanged?.Invoke();
    }

    public void AddAnimalDataToDeck(
        AnimalData animalData,
        AnimalModifier modifier = AnimalModifier.Rainbow
    )
    {
        DeckAnimal deckAnimal = new DeckAnimal(animalData, modifier);
        playerDeck.Add(deckAnimal);
        OnDeckChanged?.Invoke();
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public List<DeckAnimal> GetPlayerDeck() => new List<DeckAnimal>(playerDeck);

    public Queue<DeckAnimal> GetAnimalQueue() => new Queue<DeckAnimal>(animalQueue);

    public int GetQueueSize() => animalQueue.Count;

    public int GetDeckSize() => playerDeck.Count;

    public bool IsQueueEmpty() => animalQueue.Count == 0;

    public bool IsDeckEmpty() => playerDeck.Count == 0;
}
