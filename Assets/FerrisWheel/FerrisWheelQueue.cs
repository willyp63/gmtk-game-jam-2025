using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FerrisWheelQueue : MonoBehaviour
{
    [SerializeField]
    private Animal animalPrefab;

    [SerializeField]
    private List<Transform> animalPositions = new();

    [SerializeField]
    private Transform newAnimalPosition;

    [SerializeField]
    private FerrisWheel ferrisWheel;

    // List of current animals in the queue
    private List<Animal> queueAnimals = new();

    // Dictionary to store event handlers for each animal
    private Dictionary<Animal, System.Action> animalEventHandlers = new();

    // Events
    public System.Action OnQueueChanged;

    public void GenerateQueue()
    {
        // Clear existing queue
        ClearQueue();

        // Get animals from the deck manager
        List<AnimalData> animalDataList = DeckManager.Instance.DequeueAnimals(
            animalPositions.Count
        );

        // Create Animal instances for each animal data
        for (int i = 0; i < animalDataList.Count && i < animalPositions.Count; i++)
        {
            CreateAnimalInQueue(animalDataList[i], i);
        }

        OnQueueChanged?.Invoke();
    }

    private Animal CreateAnimalInQueue(AnimalData animalData, int position)
    {
        if (position > animalPositions.Count)
        {
            Debug.LogError(
                $"Position {position} is out of bounds. Max positions: {animalPositions.Count}"
            );
            return null;
        }

        Transform animalPosition =
            position == animalPositions.Count ? newAnimalPosition : animalPositions[position];

        Animal newAnimal = Instantiate(animalPrefab, animalPosition.position, Quaternion.identity);
        newAnimal.transform.parent = animalPosition.gameObject.transform;
        newAnimal.transform.localPosition = Vector3.zero;
        newAnimal.InitializeAnimal(animalData, ferrisWheel);

        // Create event handler and store it
        System.Action eventHandler = () =>
        {
            if (ferrisWheel.CanLoadAnimal())
            {
                ferrisWheel.LoadAnimal(newAnimal);
                RemoveAnimal(newAnimal);
            }
        };

        // Subscribe to the event
        newAnimal.OnDragToLoadingZone += eventHandler;

        // Store the event handler for later unsubscription
        animalEventHandlers[newAnimal] = eventHandler;

        // Add to queue list
        queueAnimals.Add(newAnimal);

        return newAnimal;
    }

    public void RemoveAnimal(Animal animal)
    {
        // Find the animal in our queue
        int animalIndex = queueAnimals.IndexOf(animal);
        if (animalIndex == -1)
            return;

        // Unsubscribe from the event
        if (animalEventHandlers.ContainsKey(animal))
        {
            animal.OnDragToLoadingZone -= animalEventHandlers[animal];
            animalEventHandlers.Remove(animal);
        }

        // Remove from queue
        queueAnimals.RemoveAt(animalIndex);

        // Shift remaining animals forward
        StartCoroutine(AnimateShiftAnimalsForward(animalIndex));
    }

    private IEnumerator AnimateShiftAnimalsForward(int startIndex)
    {
        float animationDuration = 0.3f; // Quick animation duration
        float elapsedTime = 0f;

        // Create new animal off-screen first if we have animals left in deck
        if (!DeckManager.Instance.IsQueueEmpty())
        {
            List<AnimalData> newAnimalData = DeckManager.Instance.DequeueAnimals(1);
            if (newAnimalData.Count > 0)
            {
                CreateAnimalInQueue(newAnimalData[0], animalPositions.Count);
            }
        }

        // Store starting positions for each animal that needs to move (including new animal)
        Dictionary<Animal, Vector3> startPositions = new Dictionary<Animal, Vector3>();
        Dictionary<Animal, Vector3> targetPositions = new Dictionary<Animal, Vector3>();

        // Calculate start and target positions for all animals that need to move
        for (int i = startIndex; i < queueAnimals.Count; i++)
        {
            if (i < animalPositions.Count)
            {
                Animal animal = queueAnimals[i];
                startPositions[animal] = animal.transform.position;
                targetPositions[animal] = animalPositions[i].position;

                animal.SetIsMoving(true);
            }
        }

        // Animate all animals moving simultaneously
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;

            // Use smooth step for more natural movement
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            // Update each animal's position simultaneously
            for (int i = startIndex; i < queueAnimals.Count; i++)
            {
                if (i < animalPositions.Count)
                {
                    Animal animal = queueAnimals[i];
                    if (startPositions.ContainsKey(animal) && targetPositions.ContainsKey(animal))
                    {
                        animal.transform.position = Vector3.Lerp(
                            startPositions[animal],
                            targetPositions[animal],
                            smoothProgress
                        );
                    }
                }
            }

            yield return null;
        }

        // Ensure animals are exactly at their target positions
        for (int i = startIndex; i < queueAnimals.Count; i++)
        {
            if (i < animalPositions.Count)
            {
                queueAnimals[i].transform.position = animalPositions[i].position;
                queueAnimals[i].UpdateSnapPosition();
                queueAnimals[i].SetIsMoving(false);
            }
        }

        OnQueueChanged?.Invoke();
    }

    private void ClearQueue()
    {
        // Unsubscribe from all events before destroying animals
        foreach (Animal animal in queueAnimals)
        {
            if (animal != null && animalEventHandlers.ContainsKey(animal))
            {
                animal.OnDragToLoadingZone -= animalEventHandlers[animal];
            }
        }

        // Clear the event handlers dictionary
        animalEventHandlers.Clear();

        // Destroy the animals
        foreach (Animal animal in queueAnimals)
        {
            if (animal != null)
            {
                Destroy(animal.gameObject);
            }
        }
        queueAnimals.Clear();
    }

    public List<Animal> GetQueueAnimals()
    {
        return new List<Animal>(queueAnimals);
    }
}
