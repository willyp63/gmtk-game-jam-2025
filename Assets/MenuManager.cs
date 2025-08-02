using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private Button startNormalButton;

    [SerializeField]
    private Button startExpertButton;

    [SerializeField]
    private Button exitButton;

    private void Start()
    {
        startNormalButton.onClick.AddListener(StartNormalGame);
        startExpertButton.onClick.AddListener(StartExpertGame);
        exitButton.onClick.AddListener(ExitGame);
    }

    private void StartExpertGame()
    {
        Debug.Log("Start Expert Game");
    }

    private void StartNormalGame()
    {
        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Load the scene
        SceneManager.LoadScene(1);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Unsubscribe from the event to avoid multiple calls
        SceneManager.sceneLoaded -= OnSceneLoaded;

        GameManager.Instance.RestartGame();
    }

    private void ExitGame()
    {
        Application.Quit();
    }
}
