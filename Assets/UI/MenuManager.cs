using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private Button startButton;

    [SerializeField]
    private Button exitButton;

    [SerializeField]
    private GameObject newRunPanel;

    [SerializeField]
    private Button rightDeckSelectButton;

    [SerializeField]
    private Button leftDeckSelectButton;

    [SerializeField]
    private Button startNewRunButton;

    [SerializeField]
    private Button cancelNewRunButton;

    [SerializeField]
    private TextMeshProUGUI deckNameText;

    [SerializeField]
    private List<Image> deckAnimalImages;

    private int selectedDeckIndex = 0;

    private void Start()
    {
        startButton.onClick.AddListener(ShowNewRunPanel);
        exitButton.onClick.AddListener(ExitGame);
        startNewRunButton.onClick.AddListener(StartNewRun);
        cancelNewRunButton.onClick.AddListener(HideNewRunPanel);
        rightDeckSelectButton.onClick.AddListener(() =>
            UpdateSelectedDeckIndex(selectedDeckIndex + 1)
        );
        leftDeckSelectButton.onClick.AddListener(() =>
            UpdateSelectedDeckIndex(selectedDeckIndex - 1)
        );

        HideNewRunPanel();
    }

    private void ShowNewRunPanel()
    {
        SFXManager.Instance.PlaySFX("button_click");

        newRunPanel.SetActive(true);
        UpdateSelectedDeckIndex(0);
    }

    private void HideNewRunPanel()
    {
        SFXManager.Instance.PlaySFX("button_click");

        newRunPanel.SetActive(false);
    }

    private void ExitGame()
    {
        SFXManager.Instance.PlaySFX("button_click");

        Application.Quit();
    }

    private void UpdateSelectedDeckIndex(int index)
    {
        if (DeckManager.Instance.StarterDecks.Count == 0)
        {
            Debug.LogError("No starter decks found");
            return;
        }

        selectedDeckIndex = Mathf.Clamp(index, 0, DeckManager.Instance.StarterDecks.Count - 1);
        UpdateSelectedDeck(DeckManager.Instance.StarterDecks[selectedDeckIndex]);
        DeckManager.Instance.SetStarterDeck(selectedDeckIndex);

        rightDeckSelectButton.interactable =
            selectedDeckIndex < DeckManager.Instance.StarterDecks.Count - 1;
        leftDeckSelectButton.interactable = selectedDeckIndex > 0;
    }

    private void UpdateSelectedDeck(DeckData deckData)
    {
        deckNameText.text = deckData.deckName;
        for (int i = 0; i < deckAnimalImages.Count; i++)
        {
            AnimalWithModifier animal = deckData.animals[i];
            if (animal != null)
            {
                deckAnimalImages[i].sprite = animal.animalData.sprite;
                deckAnimalImages[i].gameObject.SetActive(true);
            }
            else
            {
                deckAnimalImages[i].sprite = null;
                deckAnimalImages[i].gameObject.SetActive(false);
            }
        }
    }

    private void StartNewRun()
    {
        SFXManager.Instance.PlaySFX("button_click");

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(1);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameManager.Instance.RestartGame();
    }
}
