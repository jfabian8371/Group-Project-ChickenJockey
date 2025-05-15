using UnityEngine;
using TMPro;

public class EnemyHealthManager : MonoBehaviour
{
    public RectTransform healthBar;        // The red fill bar
    public float barWidth = 200f;          // Total width of the health bar
    public float barHeight = 20f;          // Height of the bar
    public TMP_Text healthText;            // Health text (e.g., 80/100)

    private float totalEnemyHealth = 0f;
    private float currentEnemyHealth = 0f;
    private int enemiesRemaining = 0;

    private void Start()
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

        foreach (EnemyHealth enemy in enemies)
        {
            totalEnemyHealth += enemy.maxHealth;
            currentEnemyHealth += enemy.currentHealth;
            enemy.manager = this;
        }

        enemiesRemaining = enemies.Length;

        UpdateHealthBar();
    }

    public void ReportDamage(float amount)
    {
        currentEnemyHealth -= amount;
        UpdateHealthBar();
    }

    public void ReportEnemyDeath(float remainingHealth)
    {
        currentEnemyHealth -= remainingHealth;
        enemiesRemaining--;

        UpdateHealthBar();

        if (enemiesRemaining <= 0)
        {
            if (healthBar != null)
                healthBar.gameObject.SetActive(false);

            if (healthText != null)
                healthText.gameObject.SetActive(false);
        }
    }

    private void UpdateHealthBar()
    {
        currentEnemyHealth = Mathf.Clamp(currentEnemyHealth, 0, totalEnemyHealth);

        float ratio = totalEnemyHealth > 0 ? currentEnemyHealth / totalEnemyHealth : 0f;
        float newWidth = ratio * barWidth;

        // Hide the ENTIRE bar object (parent) if health is 0
        bool shouldShowBar = currentEnemyHealth > 0;

        if (healthBar != null)
        {
            healthBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);

            // Instead of hiding just the red bar, hide the full parent container
            if (healthBar.transform.parent != null)
            {
                healthBar.transform.parent.gameObject.SetActive(shouldShowBar);
            }
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.RoundToInt(currentEnemyHealth)}/{Mathf.RoundToInt(totalEnemyHealth)}";
            healthText.gameObject.SetActive(shouldShowBar);
        }
    }

    public void RegisterNewWave(EnemyHealth[] newEnemies)
    {
        totalEnemyHealth = 0f;
        currentEnemyHealth = 0f;

        foreach (EnemyHealth enemy in newEnemies)
        {
            totalEnemyHealth += enemy.maxHealth;
            currentEnemyHealth += enemy.currentHealth;
            enemy.manager = this;
        }

        // Make sure the health bar and text are visible again
        if (healthBar != null)
            healthBar.gameObject.SetActive(true);
        if (healthText != null)
            healthText.gameObject.SetActive(true);

        UpdateHealthBar();
    }



}
