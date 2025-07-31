using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private void Start()
    {
        RoundManager.Instance.OnRoundStarted += OnRoundStarted;
        RoundManager.Instance.OnRoundCompleted += OnRoundCompleted;
        RoundManager.Instance.OnRoundFailed += OnRoundFailed;

        DeckManager.Instance.InitializeDeck();
        DeckManager.Instance.RegenerateQueue();
        FerrisWheelQueue.Instance.GenerateQueue();

        RoundManager.Instance.StartFirstRound();
    }

    private void OnRoundStarted()
    {
        Debug.Log("Round started");
    }

    private void OnRoundCompleted()
    {
        Debug.Log("Round completed");

        StartCoroutine(StartNextRoundAfterDelay());
    }

    private IEnumerator StartNextRoundAfterDelay()
    {
        yield return new WaitForSeconds(2f);

        FerrisWheel.Instance.ClearWheel();
        DeckManager.Instance.RegenerateQueue();
        FerrisWheelQueue.Instance.GenerateQueue();
        RoundManager.Instance.AdvanceToNextRound();
    }

    private void OnRoundFailed()
    {
        Debug.Log("Round failed");
    }
}
