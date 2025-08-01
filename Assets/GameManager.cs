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

        FerrisWheelQueue.Instance.GenerateQueue();

        RoundManager.Instance.StartFirstRound();
    }

    private void Update()
    {
        // Check for Ctrl+R input to restart the game
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
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

        FerrisWheelQueue.Instance.GenerateQueue();
        RoundManager.Instance.AdvanceToNextRound();
    }

    private void OnRoundFailed()
    {
        Debug.Log("Round failed");
        RestartGame();
    }

    private void RestartGame()
    {
        Debug.Log("Restarting game...");

        // Stop any ongoing coroutines
        StopAllCoroutines();

        // Clear the ferris wheel
        FerrisWheel.Instance.ClearWheel();

        // Reinitialize everything
        FerrisWheelQueue.Instance.GenerateQueue();

        // Start back at round 1
        RoundManager.Instance.StartFirstRound();
    }
}
