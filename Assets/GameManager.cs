using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    private FerrisWheelQueue ferrisWheelQueue;

    [SerializeField]
    private FerrisWheel ferrisWheel;

    private void Start()
    {
        // Initialize the deck manager
        DeckManager.Instance.InitializeDeck();
        DeckManager.Instance.RegenerateQueue();

        // Initialize the ferris wheel queue
        ferrisWheelQueue.GenerateQueue();

        RoundManager.Instance.OnRoundStarted += OnRoundStarted;
        RoundManager.Instance.OnRoundCompleted += OnRoundCompleted;
        RoundManager.Instance.OnRoundFailed += OnRoundFailed;

        RoundManager.Instance.StartFirstRound();
    }

    private void OnRoundStarted()
    {
        Debug.Log("Round started");
    }

    private void OnRoundCompleted()
    {
        Debug.Log("Round completed");
        RoundManager.Instance.AdvanceToNextRound();
    }

    private void OnRoundFailed()
    {
        Debug.Log("Round failed");
    }
}
