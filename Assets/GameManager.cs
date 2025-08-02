using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    private FerrisWheel ferrisWheel;
    public FerrisWheel FerrisWheel => ferrisWheel;

    private FerrisWheelQueue ferrisWheelQueue;
    public FerrisWheelQueue FerrisWheelQueue => ferrisWheelQueue;

    private void Start()
    {
        RoundManager.Instance.OnRoundStarted += OnRoundStarted;
        RoundManager.Instance.OnRoundCompleted += OnRoundCompleted;
        RoundManager.Instance.OnRoundFailed += OnRoundFailed;

        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            RestartGame();
        }
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

        ferrisWheelQueue.GenerateQueue();
        RoundManager.Instance.AdvanceToNextRound();
    }

    private void OnRoundFailed()
    {
        Debug.Log("Round failed");
        RestartGame();
    }

    public void RestartGame()
    {
        Debug.Log("Restarting game...");

        StopAllCoroutines();

        ferrisWheel = FindObjectOfType<FerrisWheel>();
        ferrisWheelQueue = FindObjectOfType<FerrisWheelQueue>();
        if (ferrisWheel == null || ferrisWheelQueue == null)
        {
            Debug.LogError("GameManager: FerrisWheel or FerrisWheelQueue not found!");
            return;
        }

        ferrisWheel.Initialize();
        UIManager.Instance.Initialize();
        RoundManager.Instance.Initialize();

        ferrisWheelQueue.GenerateQueue();
        RoundManager.Instance.StartFirstRound();
    }
}
