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

    // List of current animals in the queue
    private List<Animal> queueAnimals = new();

    // Dictionary to store event handlers for each animal
    private Dictionary<Animal, System.Action<Vector3>> animalEventHandlers = new();

    // Dictionary to store skip zone event handlers for each animal
    private Dictionary<Animal, System.Action<Vector3>> animalSkipEventHandlers = new();

    // Events
    public System.Action OnQueueChanged;

    public void GenerateQueue()
    {
        // Clear existing queue
        ClearQueue();

        // Get animals from the deck manager
        List<DeckAnimal> deckAnimalList = DeckManager.Instance.DequeueAnimals(
            animalPositions.Count
        );

        // Start the animation coroutine
        StartCoroutine(AnimateGenerateQueue(deckAnimalList));
    }

    private IEnumerator AnimateGenerateQueue(List<DeckAnimal> deckAnimalList)
    {
        // Calculate the distance from first animal position to new animal position
        float distance = Vector3.Distance(animalPositions[0].position, newAnimalPosition.position);

        // Create animals at their initial positions (off-screen to the left)
        for (int i = 0; i < deckAnimalList.Count && i < animalPositions.Count; i++)
        {
            Animal newAnimal = CreateAnimalInQueue(deckAnimalList[i], i);

            // Set initial position off-screen to the left
            Vector3 initialPosition = animalPositions[i].position - Vector3.right * distance;
            newAnimal.transform.position = initialPosition;
            newAnimal.SetIsMoving(true);
        }

        // Animate animals moving to their final positions
        float animationDuration = 1.0f;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;

            // Use smooth step for more natural movement
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            // Update each animal's position
            for (int i = 0; i < queueAnimals.Count && i < animalPositions.Count; i++)
            {
                Animal animal = queueAnimals[i];
                Vector3 startPosition = animalPositions[i].position - Vector3.right * distance;
                Vector3 targetPosition = animalPositions[i].position;

                animal.transform.position = Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    smoothProgress
                );
            }

            yield return null;
        }

        // Ensure animals are exactly at their target positions
        for (int i = 0; i < queueAnimals.Count && i < animalPositions.Count; i++)
        {
            queueAnimals[i].transform.position = animalPositions[i].position;
            queueAnimals[i].UpdateSnapPosition();
            queueAnimals[i].SetIsMoving(false);
        }

        OnQueueChanged?.Invoke();
    }

    private Animal CreateAnimalInQueue(DeckAnimal deckAnimal, int position)
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
        newAnimal.InitializeAnimal(deckAnimal);

        // Create loading zone event handler and store it
        System.Action<Vector3> loadingEventHandler = (Vector3 position) =>
        {
            if (GameManager.Instance.FerrisWheel.CanLoadAnimal())
            {
                GameManager.Instance.FerrisWheel.LoadAnimal(newAnimal);
                RemoveAnimal(newAnimal);
            }
        };

        // Create skip zone event handler and store it
        System.Action<Vector3> skipEventHandler = (Vector3 position) =>
        {
            SkipAnimal(newAnimal, position);
        };

        // Subscribe to the events
        newAnimal.OnDragToLoadingZone += loadingEventHandler;
        newAnimal.OnDragToSkipZone += skipEventHandler;

        // Store the event handlers for later unsubscription
        animalEventHandlers[newAnimal] = loadingEventHandler;
        animalSkipEventHandlers[newAnimal] = skipEventHandler;

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

        // Unsubscribe from the events
        if (animalEventHandlers.ContainsKey(animal))
        {
            animal.OnDragToLoadingZone -= animalEventHandlers[animal];
            animalEventHandlers.Remove(animal);
        }

        if (animalSkipEventHandlers.ContainsKey(animal))
        {
            animal.OnDragToSkipZone -= animalSkipEventHandlers[animal];
            animalSkipEventHandlers.Remove(animal);
        }

        // Remove from queue
        queueAnimals.RemoveAt(animalIndex);

        // Shift remaining animals forward
        StartCoroutine(AnimateShiftAnimalsForward(animalIndex));
    }

    public void SkipAnimal(Animal animal, Vector3 dragToPosition)
    {
        // Find the animal in our queue
        int animalIndex = queueAnimals.IndexOf(animal);
        if (animalIndex == -1)
            return;

        // Consume a skip
        if (!RoundManager.Instance.ConsumeSkip())
        {
            return;
        }

        // Unsubscribe from the events
        if (animalEventHandlers.ContainsKey(animal))
        {
            animal.OnDragToLoadingZone -= animalEventHandlers[animal];
            animalEventHandlers.Remove(animal);
        }

        if (animalSkipEventHandlers.ContainsKey(animal))
        {
            animal.OnDragToSkipZone -= animalSkipEventHandlers[animal];
            animalSkipEventHandlers.Remove(animal);
        }

        // Remove from queue
        queueAnimals.RemoveAt(animalIndex);

        animal.transform.position = new Vector3(
            dragToPosition.x,
            GameManager.Instance.FerrisWheel.UnloadingLocation.position.y,
            0f
        );

        // Animate the skipped animal off screen
        StartCoroutine(AnimateSkippedAnimal(animal));

        // Shift remaining animals forward
        StartCoroutine(AnimateShiftAnimalsForward(animalIndex));
    }

    private IEnumerator AnimateSkippedAnimal(Animal animal)
    {
        if (animal == null || GameManager.Instance.FerrisWheel.UnloadingLocation == null)
        {
            Destroy(animal?.gameObject);
            yield break;
        }

        animal.SetIsMoving(true);

        Vector3 targetPosition = animal.transform.position + Vector3.right * 12f;

        // Animate animal to the target position
        float moveToPositionDuration = 1.0f;
        float elapsedTime = 0f;
        Vector3 startPosition = animal.transform.position;

        while (elapsedTime < moveToPositionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveToPositionDuration;
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            animal.transform.position = Vector3.Lerp(startPosition, targetPosition, easedT);
            yield return null;
        }

        // Ensure animal is exactly at target position
        animal.transform.position = targetPosition;

        // Animate animal moving to the right 12 units (same as unloading)
        Vector3 finalPosition = targetPosition + Vector3.right * 12f;
        float moveRightDuration = 2.0f;
        elapsedTime = 0f;

        while (elapsedTime < moveRightDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveRightDuration;
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            animal.transform.position = Vector3.Lerp(targetPosition, finalPosition, easedT);
            yield return null;
        }

        // Destroy the animal
        Destroy(animal.gameObject);
    }

    private IEnumerator AnimateShiftAnimalsForward(int startIndex)
    {
        float animationDuration = 0.5f; // Quick animation duration
        float elapsedTime = 0f;

        // Create new animal off-screen first
        List<DeckAnimal> newDeckAnimal = DeckManager.Instance.DequeueAnimals(1);
        if (newDeckAnimal.Count > 0)
        {
            CreateAnimalInQueue(newDeckAnimal[0], animalPositions.Count);
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
            if (animal != null && animalSkipEventHandlers.ContainsKey(animal))
            {
                animal.OnDragToSkipZone -= animalSkipEventHandlers[animal];
            }
        }

        // Clear the event handlers dictionary
        animalEventHandlers.Clear();
        animalSkipEventHandlers.Clear();

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
