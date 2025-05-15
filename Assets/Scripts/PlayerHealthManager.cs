using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthManager : MonoBehaviour
{
    [Header("Bars")]
    public RectTransform healthBar;
    public RectTransform shieldBar;

    [Header("Settings")]
    public float maxHealth = 100f;
    public float maxShield = 100f;

    public float barWidth = 155;  // Total width to stay within
    public float barHeight = 20f;

    private float currentHealth;
    private float currentShield;

    private void Start()
    {
        currentHealth = maxHealth;
        currentShield = maxShield;

        UpdateBars();
    }

    public void TakeDamage(float amount)
    {
        if (currentShield > 0)
        {
            float shieldDamage = Mathf.Min(currentShield, amount);
            currentShield -= shieldDamage;
            amount -= shieldDamage;
        }

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateBars();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateBars();
    }

    public void RechargeShield(float amount)
    {
        currentShield = Mathf.Clamp(currentShield + amount, 0, maxShield);
        UpdateBars();
    }

    private void UpdateBars()
    {
        if (healthBar != null)
        {
            float healthRatio = currentHealth / maxHealth;
            healthBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, healthRatio * barWidth);
        }

        if (shieldBar != null)
        {
            float shieldRatio = currentShield / maxShield;
            shieldBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, shieldRatio * barWidth);
        }
    }
}
