using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [SerializeField]
    private TextMeshProUGUI roundText;

    [SerializeField]
    private TextMeshProUGUI requiredPointsText;

    [SerializeField]
    private TextMeshProUGUI pointsText;

    [SerializeField]
    private TextMeshProUGUI energyText;

    [SerializeField]
    private TextMeshProUGUI skipsText;

    [SerializeField]
    private List<Button> rotateButtons;

    [SerializeField]
    private Button endDayEarlyButton;

    [SerializeField]
    private Button menuButton;

    [SerializeField]
    private Button helpButton;

    [SerializeField]
    private string helpTitle = "Welcome to Wacky Wharf!";

    [SerializeField]
    [TextArea(8, 12)]
    private string helpText = "";

    [SerializeField]
    private GameObject pointsIndicatorPrefab;

    [SerializeField]
    private Canvas worldSpaceCanvas;

    [SerializeField]
    private float pointsIndicatorYOffset = 2f;

    [SerializeField]
    private float pointsIndicatorXOffset = 0f;

    [SerializeField]
    private GameObject dialogPanel;

    [SerializeField]
    private TextMeshProUGUI dialogTitleText;

    [SerializeField]
    private TextMeshProUGUI dialogBodyText;

    [SerializeField]
    private TextMeshProUGUI dialogConfirmButtonText;

    [SerializeField]
    private TextMeshProUGUI dialogCancelButtonText;

    [SerializeField]
    private Button dialogConfirmButton;

    [SerializeField]
    private Button dialogCancelButton;

    // Dialog callback delegate
    public delegate void DialogCallback();

    // Current dialog callback
    private DialogCallback currentDialogCallback;

    private List<PointsIndicator> pointsIndicators = new List<PointsIndicator>();

    public void Initialize()
    {
        InitializePointsIndicators();
        InitializeRotateButtons();
        SubscribeToRoundManagerEvents();
        UpdateAllDisplays();
        UpdateButtonStates();
        HideDialog();
    }

    private void SubscribeToRoundManagerEvents()
    {
        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.OnScoreChanged += UpdateScoreDisplay;
            RoundManager.Instance.OnEnergyChanged += UpdateEnergyDisplay;
            RoundManager.Instance.OnSkipsChanged += UpdateSkipsDisplay;
            RoundManager.Instance.OnRoundStarted += UpdateRoundDisplay;
        }
    }

    private void UpdateAllDisplays()
    {
        UpdateRoundDisplay();
        UpdateScoreDisplay();
        UpdateEnergyDisplay();
        UpdateSkipsDisplay();
    }

    private void UpdateRoundDisplay()
    {
        if (roundText != null)
        {
            roundText.text = $"DAY {RoundManager.Instance.CurrentRound}";
        }
    }

    private void UpdateScoreDisplay()
    {
        if (pointsText != null)
        {
            pointsText.text = $"{RoundManager.Instance.CurrentScore:N0}";
        }

        if (requiredPointsText != null)
        {
            requiredPointsText.text = $"{RoundManager.Instance.RequiredScore:N0}";
        }
    }

    private void UpdateEnergyDisplay()
    {
        if (energyText != null)
        {
            energyText.text = $"{RoundManager.Instance.CurrentEnergy}";
        }
    }

    private void UpdateSkipsDisplay()
    {
        if (skipsText != null)
        {
            skipsText.text = $"{RoundManager.Instance.SkipsRemaining}";
        }
    }

    private void UpdateButtonStates()
    {
        bool wheelIsRotating =
            GameManager.Instance.FerrisWheel != null && GameManager.Instance.FerrisWheel.IsRotating;
        int currentEnergy = RoundManager.Instance.CurrentEnergy;

        // Update rotate buttons
        for (int i = 0; i < rotateButtons.Count; i++)
        {
            if (rotateButtons[i] != null)
            {
                int energyRequired = i + 1; // First button = 1 energy, second button = 2 energy, etc.
                bool hasEnoughEnergy = currentEnergy >= energyRequired;
                bool canRotate = !wheelIsRotating && hasEnoughEnergy;

                rotateButtons[i].interactable = canRotate;
            }
        }

        // Update end day early button
        if (endDayEarlyButton != null)
        {
            bool canEndDay = !wheelIsRotating && currentEnergy > 0;
            endDayEarlyButton.interactable = canEndDay;
        }
    }

    private void Update()
    {
        UpdatePointsIndicators();
        CheckRotateButtons();
        UpdateButtonStates();
    }

    private void CheckRotateButtons()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            OnRotateButtonClicked(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            OnRotateButtonClicked(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            OnRotateButtonClicked(3);
        }
    }

    private void InitializePointsIndicators()
    {
        if (
            GameManager.Instance.FerrisWheel == null
            || pointsIndicatorPrefab == null
            || worldSpaceCanvas == null
        )
        {
            Debug.LogError("UIManager: Missing required references!");
            return;
        }

        // Clear any existing indicators
        foreach (var indicator in pointsIndicators)
        {
            if (indicator != null)
            {
                Destroy(indicator.gameObject);
            }
        }
        pointsIndicators.Clear();

        // Get all cart hinges from the FerrisWheel
        List<Cart> carts = GameManager.Instance.FerrisWheel.Carts;

        // Create a points indicator for each cart hinge
        foreach (Cart cart in carts)
        {
            if (cart.Hinge != null)
            {
                GameObject indicatorObject = Instantiate(
                    pointsIndicatorPrefab,
                    worldSpaceCanvas.transform
                );
                PointsIndicator indicator = indicatorObject.GetComponent<PointsIndicator>();

                if (indicator != null)
                {
                    indicator.Initialize(
                        cart,
                        worldSpaceCanvas,
                        pointsIndicatorXOffset,
                        pointsIndicatorYOffset
                    );
                    pointsIndicators.Add(indicator);
                }
            }
        }
    }

    private void UpdatePointsIndicators()
    {
        foreach (var indicator in pointsIndicators)
        {
            if (indicator != null)
            {
                indicator.UpdatePosition();
                indicator.UpdatePoints();
            }
        }
    }

    private void InitializeRotateButtons()
    {
        if (rotateButtons == null || rotateButtons.Count == 0)
        {
            Debug.LogWarning("UIManager: No rotate buttons assigned!");
            return;
        }

        for (int i = 0; i < rotateButtons.Count; i++)
        {
            int steps = i + 1; // First button = 1 step, second button = 2 steps, etc.
            Button button = rotateButtons[i];

            if (button != null)
            {
                button.onClick.AddListener(() => OnRotateButtonClicked(steps));
            }
        }

        if (endDayEarlyButton != null)
        {
            endDayEarlyButton.onClick.AddListener(OnEndDayEarlyButtonClicked);
        }

        if (menuButton != null)
        {
            menuButton.onClick.AddListener(OnMenuButtonClicked);
        }

        if (helpButton != null)
        {
            helpButton.onClick.AddListener(ShowHelpDialog);
        }
    }

    public void ShowHelpDialog()
    {
        ShowDialog(helpTitle, helpText, "DISMISS", "", 950f, () => { });
    }

    private void OnMenuButtonClicked()
    {
        ShowDialog(
            "Main Menu?",
            "You will lose all your progress and have to start over.",
            "MAIN MENU",
            "CANCEL",
            400f,
            () =>
            {
                SceneManager.LoadScene(0);
            }
        );
    }

    private void OnEndDayEarlyButtonClicked()
    {
        if (GameManager.Instance.FerrisWheel != null)
        {
            GameManager.Instance.FerrisWheel.EndDayEarly();
        }
    }

    private void OnRotateButtonClicked(int steps)
    {
        if (GameManager.Instance.FerrisWheel != null)
        {
            GameManager.Instance.FerrisWheel.RotateWheel(false, steps);
        }
    }

    public void ShowDialog(
        string title,
        string body,
        string confirmButtonText,
        string cancelButtonText,
        float dialogHeight = 500f,
        DialogCallback callback = null
    )
    {
        if (dialogPanel != null)
        {
            // Set the dialog content
            if (dialogTitleText != null)
                dialogTitleText.text = title.ToUpper();

            if (dialogBodyText != null)
                dialogBodyText.text = body;

            if (dialogConfirmButtonText != null)
                dialogConfirmButtonText.text = confirmButtonText.ToUpper();

            if (dialogCancelButtonText != null)
                dialogCancelButtonText.text = cancelButtonText.ToUpper();

            // Store the callback
            currentDialogCallback = callback;

            // Set up the button click listener
            if (dialogConfirmButton != null)
            {
                dialogConfirmButton.onClick.RemoveAllListeners();
                dialogConfirmButton.onClick.AddListener(OnDialogButtonClicked);
                dialogConfirmButton.gameObject.SetActive(confirmButtonText != "");
            }

            if (dialogCancelButton != null)
            {
                dialogCancelButton.onClick.RemoveAllListeners();
                dialogCancelButton.onClick.AddListener(HideDialog);
                dialogCancelButton.gameObject.SetActive(cancelButtonText != "");
            }

            // Show the dialog
            dialogPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(
                dialogPanel.GetComponent<RectTransform>().sizeDelta.x,
                dialogHeight
            );
            dialogPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("UIManager: Dialog panel is not assigned!");
        }
    }

    public void HideDialog()
    {
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
            currentDialogCallback = null;
        }
    }

    private void OnDialogButtonClicked()
    {
        // Invoke the callback if one was provided
        if (currentDialogCallback != null)
        {
            currentDialogCallback.Invoke();
        }

        // Hide the dialog
        HideDialog();
    }
}
