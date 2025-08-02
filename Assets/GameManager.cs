using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    [TextArea(8, 12)]
    private string roundCompletedDialogBody = "";

    [SerializeField]
    [TextArea(8, 12)]
    private string roundFailedDialogBody = "";

    [SerializeField]
    private bool hasSeenHelp = false;

    private FerrisWheel ferrisWheel;
    public FerrisWheel FerrisWheel => ferrisWheel;

    private FerrisWheelQueue ferrisWheelQueue;
    public FerrisWheelQueue FerrisWheelQueue => ferrisWheelQueue;

    private void Start()
    {
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

    public void CompleteRound()
    {
        // Show completion dialog
        UIManager.Instance.ShowDialog(
            "Excellent Work!",
            roundCompletedDialogBody
                .Replace("#SCORE#", $"{RoundManager.Instance.CurrentScore:N0}")
                .Replace("#REQUIRED#", $"{RoundManager.Instance.RequiredScore:N0}"),
            "NEXT DAY",
            "",
            550f,
            () =>
            {
                // Proceed to next round after user clicks button
                ferrisWheelQueue.GenerateQueue();
                RoundManager.Instance.AdvanceToNextRound();
            }
        );
    }

    public void FailRound()
    {
        Debug.Log("Round failed");

        // Show failure dialog
        UIManager.Instance.ShowDialog(
            "You're Fired!",
            roundFailedDialogBody
                .Replace("#SCORE#", $"{RoundManager.Instance.CurrentScore:N0}")
                .Replace("#REQUIRED#", $"{RoundManager.Instance.RequiredScore:N0}"),
            "TRY AGAIN",
            "",
            550f,
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
        ferrisWheel.ClearWheel();
        UIManager.Instance.Initialize();
        RoundManager.Instance.Initialize();

        ferrisWheelQueue.GenerateQueue();
        RoundManager.Instance.StartFirstRound();

        Debug.Log("hasSeenHelp: " + hasSeenHelp);
        if (!hasSeenHelp)
        {
            StartCoroutine(ShowHelpDialog());
        }
    }

    public IEnumerator ShowHelpDialog()
    {
        yield return new WaitForSeconds(1f);
        UIManager.Instance.ShowHelpDialog();
        hasSeenHelp = true;
    }
}
