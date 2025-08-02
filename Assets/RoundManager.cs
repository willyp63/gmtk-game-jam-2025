using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RoundManager : Singleton<RoundManager>
{
    [SerializeField]
    private int initialRequiredScore = 50;

    [SerializeField]
    private int requiredScoreIncreasePerRound = 10;

    [SerializeField]
    private int energyPerDay = 16;

    [SerializeField]
    private int energyIncreasePerRound = 2;

    [SerializeField]
    private int skipsPerRound = 2;

    // Current state
    private int currentRound = 0;
    private int currentEnergy = 0;
    private int currentScore = 0;
    private int requiredScore = 0;
    private int currentSkipsUsed = 0;
    private int maxEnergy = 0;
    private bool lightsAreOn = false;

    // Events
    public event Action OnScoreChanged;
    public event Action OnEnergyChanged;
    public event Action OnSkipsChanged;
    public event Action OnRoundStarted;

    // Properties
    public int CurrentRound => currentRound;
    public int CurrentEnergy => currentEnergy;
    public int CurrentScore => currentScore;
    public int RequiredScore => requiredScore;
    public int EnergyPerDay => energyPerDay;
    public int SkipsPerRound => skipsPerRound;
    public int CurrentSkipsUsed => currentSkipsUsed;
    public int SkipsRemaining => skipsPerRound - currentSkipsUsed;
    public bool IsRoundComplete => currentScore >= requiredScore;
    public bool CanSkip => currentSkipsUsed < skipsPerRound;

    public void Initialize()
    {
        GameManager.Instance.FerrisWheel.OnWheelStopped += OnWheelStopped;
        maxEnergy = energyPerDay;
    }

    public void StartFirstRound()
    {
        currentRound = 1;
        requiredScore = initialRequiredScore;

        ResetRound(true);

        OnRoundStarted?.Invoke();
    }

    public void AdvanceToNextRound()
    {
        currentRound++;
        requiredScore += requiredScoreIncreasePerRound;

        ResetRound();

        OnRoundStarted?.Invoke();
    }

    private void OnWheelStopped()
    {
        Debug.Log("OnWheelStopped");
        if (currentEnergy <= 0)
        {
            if (IsRoundComplete)
            {
                Debug.Log("OnRoundCompleted");
                GameManager.Instance.CompleteRound();
            }
            else
            {
                Debug.Log("OnRoundFailed");
                GameManager.Instance.FailRound();
            }
        }
    }

    private void ResetRound(bool isFirstRound = false)
    {
        currentScore = 0;
        currentEnergy = energyPerDay + ((currentRound - 1) * energyIncreasePerRound);
        maxEnergy = currentEnergy;
        currentSkipsUsed = 0;
        lightsAreOn = false;

        // Turn off all lights at the start of a new day
        LightManager.Instance.SetLightsActive(false);

        OnScoreChanged?.Invoke();
        OnEnergyChanged?.Invoke();
        OnSkipsChanged?.Invoke();

        UpdateLighting(isFirstRound ? 0f : 0.5f);
    }

    public void AddScore(int points)
    {
        currentScore += points;
        OnScoreChanged?.Invoke();
    }

    public bool ConsumeEnergy(int amount)
    {
        if (amount <= 0)
            return true;
        if (currentEnergy < amount)
            return false;

        currentEnergy -= amount;
        OnEnergyChanged?.Invoke();

        float transitionDuration =
            amount <= 3 ? GameManager.Instance.FerrisWheel.GetRotationDuration(amount) : 0.5f;
        UpdateLighting(transitionDuration);

        return true;
    }

    private void UpdateLighting(float transitionDuration)
    {
        float energyRatio = maxEnergy > 0 ? (float)currentEnergy / maxEnergy : 0f;
        float timeRatio = 1 - energyRatio;

        LightManager.Instance.UpdateLighting(timeRatio, transitionDuration);
    }

    public bool ConsumeSkip()
    {
        if (!CanSkip)
            return false;

        currentSkipsUsed++;
        OnSkipsChanged?.Invoke();

        return true;
    }

    public bool HasEnoughEnergy(int requiredAmount)
    {
        return currentEnergy >= requiredAmount;
    }
}
