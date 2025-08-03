using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private Button startNormalButton;

    [SerializeField]
    private Button exitButton;

    private void Start()
    {
        startNormalButton.onClick.AddListener(StartNormalGame);
        exitButton.onClick.AddListener(ExitGame);
    }

    private void StartNormalGame()
    {
        SFXManager.Instance.PlaySFX("button_click");

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
        SFXManager.Instance.PlaySFX("button_click");

        Application.Quit();
    }
}
