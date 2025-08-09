using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RoundManager : Singleton<RoundManager>
{
    [SerializeField]
    private List<int> requiredScores;

    [SerializeField]
    private int energyPerDay = 16;

    [SerializeField]
    private int energyIncreasePerRound = 2;

    [SerializeField]
    private int skipsPerRound = 2;

    [SerializeField]
    private int skipsPerRoundIncreasePerRound = 1;

    [SerializeField]
    private float initialModifierChance = 0f;

    [SerializeField]
    private int allowModifiersOnRound = 3;

    [SerializeField]
    private float modifierChanceIncreasePerRound = 0.1f;

    [SerializeField]
    private float maxModifierChance = 0.5f;

    [SerializeField]
    private int allowUncommonOnRound = 2;

    [SerializeField]
    private int allowRareOnRound = 5;

    // Current state
    private int currentRound = 0;
    private int currentEnergy = 0;
    private int currentScore = 0;
    private int requiredScore = 0;
    private int currentSkipsUsed = 0;
    private int maxEnergy = 0;
    private int maxSkips = 0;
    private float modifierChance = 0f;
    private List<Rarity> allowedRarities = new List<Rarity> { Rarity.Common };
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
    public int SkipsRemaining => maxSkips - currentSkipsUsed;
    public float ModifierChance => modifierChance;
    public List<Rarity> AllowedRarities => allowedRarities;
    public bool IsRoundComplete => currentScore >= requiredScore;
    public bool CanSkip => currentSkipsUsed < maxSkips;

    public void Initialize()
    {
        GameManager.Instance.FerrisWheel.OnWheelStopped += OnWheelStopped;
    }

    public void StartFirstRound()
    {
        currentRound = 1;
        maxEnergy = energyPerDay;
        maxSkips = skipsPerRound;
        requiredScore = requiredScores[0];
        modifierChance = initialModifierChance;
        allowedRarities = new List<Rarity> { Rarity.Common };

        ResetRound(true);

        OnRoundStarted?.Invoke();
    }

    public void AdvanceToNextRound()
    {
        currentRound++;
        requiredScore = requiredScores[currentRound - 1];

        if (currentRound >= allowModifiersOnRound)
        {
            modifierChance += modifierChanceIncreasePerRound;
            modifierChance = Mathf.Clamp(modifierChance, 0f, maxModifierChance);
        }

        if (currentRound >= allowUncommonOnRound && !allowedRarities.Contains(Rarity.Uncommon))
        {
            allowedRarities.Add(Rarity.Uncommon);
        }

        if (currentRound >= allowRareOnRound && !allowedRarities.Contains(Rarity.Rare))
        {
            allowedRarities.Add(Rarity.Rare);
        }

        ResetRound();

        OnRoundStarted?.Invoke();
    }

    private void OnWheelStopped()
    {
        StartCoroutine(CheckIfRoundIsCompleteAfterDelay());
    }

    private IEnumerator CheckIfRoundIsCompleteAfterDelay()
    {
        if (currentEnergy > 0)
            yield break;

        yield return new WaitForSeconds(0.5f);

        if (IsRoundComplete)
        {
            Debug.Log("OnRoundCompleted");
            SFXManager.Instance.PlaySFX("success");
            GameManager.Instance.CompleteRound();
        }
        else
        {
            Debug.Log("OnRoundFailed");
            SFXManager.Instance.PlaySFX("fail");
            GameManager.Instance.FailRound();
        }
    }

    private void ResetRound(bool isFirstRound = false)
    {
        currentScore = 0;
        currentEnergy = energyPerDay + ((currentRound - 1) * energyIncreasePerRound);
        maxEnergy = currentEnergy;
        maxSkips = skipsPerRound + ((currentRound - 1) * skipsPerRoundIncreasePerRound);
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
