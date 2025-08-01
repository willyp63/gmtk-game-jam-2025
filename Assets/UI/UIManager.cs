using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
    private GameObject pointsIndicatorPrefab;

    [SerializeField]
    private Canvas worldSpaceCanvas;

    [SerializeField]
    private float pointsIndicatorYOffset = 2f;

    [SerializeField]
    private float pointsIndicatorXOffset = 0f;

    private List<PointsIndicator> pointsIndicators = new List<PointsIndicator>();

    private void Start()
    {
        InitializePointsIndicators();
        InitializeRotateButtons();
        SubscribeToRoundManagerEvents();
        UpdateAllDisplays();
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

    private void Update()
    {
        UpdatePointsIndicators();
        CheckRotateButtons();
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
            FerrisWheel.Instance == null
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
        List<Cart> carts = FerrisWheel.Instance.Carts;

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
    }

    private void OnEndDayEarlyButtonClicked()
    {
        if (FerrisWheel.Instance != null)
        {
            FerrisWheel.Instance.EndDayEarly();
        }
    }

    private void OnRotateButtonClicked(int steps)
    {
        if (FerrisWheel.Instance != null)
        {
            FerrisWheel.Instance.RotateWheel(false, steps);
        }
    }
}
