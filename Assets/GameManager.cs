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

    private void OnRoundCompleted()
    {
        // Show completion dialog
        UIManager.Instance.ShowDialog(
            "Day Complete!",
            $"Great job! You completed Day {RoundManager.Instance.CurrentRound} with {RoundManager.Instance.CurrentScore:N0} points.",
            "NEXT DAY",
            "",
            500f,
            () =>
            {
                // Proceed to next round after user clicks button
                ferrisWheelQueue.GenerateQueue();
                RoundManager.Instance.AdvanceToNextRound();
            }
        );
    }

    private void OnRoundFailed()
    {
        Debug.Log("Round failed");

        // Show failure dialog
        UIManager.Instance.ShowDialog(
            "Day Failed",
            $"You didn't reach the required score of {RoundManager.Instance.RequiredScore:N0} points. Try again!",
            "RESTART",
            "",
            500f,
            () =>
            {
                // Restart the game after user clicks button
                RestartGame();
            }
        );
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
