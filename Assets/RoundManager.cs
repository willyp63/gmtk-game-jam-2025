using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Events
    public event Action OnScoreChanged;
    public event Action OnEnergyChanged;
    public event Action OnSkipsChanged;
    public event Action OnRoundCompleted;
    public event Action OnRoundFailed;
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

    private void Start()
    {
        FerrisWheel.Instance.OnWheelStopped += OnWheelStopped;
    }

    public void StartFirstRound()
    {
        currentRound = 1;
        requiredScore = initialRequiredScore;

        ResetRound();

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
        if (currentEnergy <= 0)
        {
            if (IsRoundComplete)
            {
                OnRoundCompleted?.Invoke();
            }
            else
            {
                OnRoundFailed?.Invoke();
            }
        }
    }

    private void ResetRound()
    {
        currentScore = 0;
        currentEnergy = energyPerDay + ((currentRound - 1) * energyIncreasePerRound);
        currentSkipsUsed = 0;

        OnScoreChanged?.Invoke();
        OnEnergyChanged?.Invoke();
        OnSkipsChanged?.Invoke();
    }

    public void AddScore(int points)
    {
        if (points <= 0)
            return;

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

        return true;
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
