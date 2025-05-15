using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthManager : MonoBehaviour
{
    [Header("Bars")]
    public RectTransform healthBar;
    public RectTransform shieldBar;

    [Header("Shield Settings")]
    public float maxShield = 100f;
    public float barWidth = 155f;

    private float currentShield;

    private PlayerHealth playerHealth;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth component not found on the same GameObject.");
            return;
        }

        currentShield = maxShield;
        playerHealth.OnHealthChanged += UpdateHealthBar;
    }

    private void Start()
    {
        UpdateBars();
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealthBar;
    }

    public void TakeDamage(float amount)
    {
        if (currentShield > 0)
        {
            float shieldDamage = Mathf.Min(currentShield, amount);
            currentShield -= shieldDamage;
            amount -= shieldDamage;
        }

        playerHealth.TakeDamage(amount);
        UpdateBars();
    }

    public void Heal(float amount)
    {
        playerHealth.Heal(amount);
    }

    public void RechargeShield(float amount)
    {
        currentShield = Mathf.Clamp(currentShield + amount, 0, maxShield);
        UpdateBars();
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        float healthRatio = currentHealth / maxHealth;
        healthBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, healthRatio * barWidth);
    }

    private void UpdateBars()
    {
        UpdateHealthBar(playerHealth.CurrentHealth, playerHealth.MaxHealth);

        if (shieldBar != null)
        {
            float shieldRatio = currentShield / maxShield;
            shieldBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, shieldRatio * barWidth);
        }
    }
}
