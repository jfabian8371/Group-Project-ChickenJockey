using UnityEngine;
using StarterAssets; // For FirstPersonController if we need to reference it directly (optional for now)

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Stats")]
    [SerializeField] private float _maxHealth = 100f;
    private float _currentHealth;

    [Header("Damage Reduction")]
    [Tooltip("Current damage reduction percentage (0.0 to 1.0, e.g., 0.1 for 10%)")]
    [SerializeField] private float _damageReductionPercentage = 0f; // Stored as 0.0 to 1.0

    // Optional: Reference to player controller for disabling movement/input on death
    // public FirstPersonController playerController;

    // Events (optional, but good for UI or other systems to react to health changes)
    public delegate void HealthChangedDelegate(float currentHealth, float maxHealth);
    public event HealthChangedDelegate OnHealthChanged;

    public delegate void PlayerDiedDelegate();
    public event PlayerDiedDelegate OnPlayerDied;

    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _maxHealth;
    public float DamageReductionPercentage => _damageReductionPercentage;

    void Awake()
    {
        // If you have a FirstPersonController script on the same GameObject:
        // playerController = GetComponent<FirstPersonController>();
        _currentHealth = _maxHealth;
    }

    void Start()
    {
        // Invoke initial health changed event for UI updates
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    public bool IsAtFullHealth()
    {
        return _currentHealth >= _maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (_currentHealth <= 0) return; // Already dead

        // Apply damage reduction
        float actualDamageTaken = amount * (1f - _damageReductionPercentage);
        if (actualDamageTaken < 0) actualDamageTaken = 0; // Cannot heal from damage

        _currentHealth -= actualDamageTaken;
        Debug.Log($"Player took {actualDamageTaken} damage (raw: {amount}). Health: {_currentHealth}/{_maxHealth}");

        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            Die();
        }
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    public void Heal(float amount)
    {
        _currentHealth += amount;
        if (_currentHealth > _maxHealth)
        {
            _currentHealth = _maxHealth;
        }
        Debug.Log($"Player healed by {amount}. Health: {_currentHealth}/{_maxHealth}");
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    public void HealToFull()
    {
        _currentHealth = _maxHealth;
        Debug.Log($"Player healed to full. Health: {_currentHealth}/{_maxHealth}");
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    public void IncreaseMaxHealth(float amount)
    {
        if (amount <= 0) return;
        float oldMaxHealth = _maxHealth;
        _maxHealth += amount;
        // Optionally, heal the player by the amount their max health increased
        // or scale current health proportionally. Let's heal by the amount increased for simplicity.
        _currentHealth += amount;
        if (_currentHealth > _maxHealth) // Ensure we don't exceed new max health if already full
        {
            _currentHealth = _maxHealth;
        }

        Debug.Log($"Player max health increased by {amount}. New Max Health: {_maxHealth}. Health: {_currentHealth}/{_maxHealth}");
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    public void IncreaseDamageReduction(float percentageIncrease) // percentageIncrease is 0.0 to 1.0
    {
        if (percentageIncrease <= 0) return;

        // Prevent increasing if already at or above cap
        if (_damageReductionPercentage >= 0.5f)
        {
            Debug.Log($"Player damage reduction already at cap (50%). No increase applied.");
            // Ensure it's exactly at cap if it somehow went over (shouldn't happen with this check)
            _damageReductionPercentage = 0.5f;
            return;
        }

        _damageReductionPercentage += percentageIncrease;

        if (_damageReductionPercentage > 0.5f) // Cap damage reduction at 50%
        {
            _damageReductionPercentage = 0.5f;
        }
        Debug.Log($"Player damage reduction increased. New DR: {_damageReductionPercentage * 100}%");
    }

    // Helper method for UpgradeUIManager to check if DR is capped
    public bool IsDamageReductionCapped()
    {
        return _damageReductionPercentage >= 0.5f;
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        OnPlayerDied?.Invoke();

        // --- Add death logic here ---
        // For example:
        // - Disable player input/controller (if (playerController) playerController.enabled = false;)
        // - Play death animation
        // - Show game over screen
        // - Notify GameManager to handle game over state
        // For now, we'll just log it.
    }

    // --- For Testing Purposes (can be removed later) ---
    // [ContextMenu("Test Take 10 Damage")]
    // void TestDamage()
    // {
    //     TakeDamage(10);
    // }
    // [ContextMenu("Test Heal 10")]
    // void TestHeal()
    // {
    //     Heal(10);
    // }
    // [ContextMenu("Test Heal To Full")]
    // void TestHealFull()
    // {
    //     HealToFull();
    // }
}