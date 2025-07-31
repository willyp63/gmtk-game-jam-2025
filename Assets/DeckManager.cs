using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeckManager : Singleton<DeckManager>
{
    [SerializeField]
    private List<AnimalData> initialDeck = new();

    // Player's full deck of animals
    private List<AnimalData> playerDeck = new();

    // Daily queue of animals (shuffled from deck)
    private Queue<AnimalData> animalQueue = new();

    // Events
    public System.Action OnDeckChanged;
    public System.Action OnQueueRegenerated;

    public void InitializeDeck()
    {
        playerDeck.Clear();
        playerDeck.AddRange(initialDeck);
        OnDeckChanged?.Invoke();
    }

    public void RegenerateQueue()
    {
        animalQueue.Clear();

        // Create a shuffled list from the deck
        List<AnimalData> shuffledDeck = new List<AnimalData>(playerDeck);
        ShuffleList(shuffledDeck);

        // Fill the queue up
        for (int i = 0; i < shuffledDeck.Count; i++)
        {
            animalQueue.Enqueue(shuffledDeck[i]);
        }

        OnQueueRegenerated?.Invoke();
    }

    public List<AnimalData> DequeueAnimals(int count)
    {
        List<AnimalData> result = new List<AnimalData>();

        for (int i = 0; i < count && animalQueue.Count > 0; i++)
        {
            result.Add(animalQueue.Dequeue());
        }

        return result;
    }

    public void AddAnimalToDeck(AnimalData animal)
    {
        playerDeck.Add(animal);
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

    public List<AnimalData> GetPlayerDeck() => new List<AnimalData>(playerDeck);

    public Queue<AnimalData> GetAnimalQueue() => new Queue<AnimalData>(animalQueue);

    public int GetQueueSize() => animalQueue.Count;

    public int GetDeckSize() => playerDeck.Count;

    public bool IsQueueEmpty() => animalQueue.Count == 0;

    public bool IsDeckEmpty() => playerDeck.Count == 0;
}
