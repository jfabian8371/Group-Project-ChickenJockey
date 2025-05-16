using UnityEngine;
using TMPro;

public class EnemyHealthManager : MonoBehaviour
{
    // giving this class a reference to GameManager just to call GameManager.EndRound()
    public GameManager gameManager; 

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

        totalEnemyHealth = 0f;
        currentEnemyHealth = 0f;

        foreach (EnemyHealth enemy in enemies)
        {
            totalEnemyHealth += enemy.maxHealth;
            currentEnemyHealth += enemy.currentHealth;
            enemy.healthManager = this;
        }

        enemiesRemaining = enemies.Length;

        UpdateHealthBar();
    }

    public void ReportDamage(float amount)
    {
        currentEnemyHealth -= amount;
        currentEnemyHealth = Mathf.Clamp(currentEnemyHealth, 0f, totalEnemyHealth);
        UpdateHealthBar();
    }

    // Called when an enemy dies
    public void ReportEnemyDeath()
    {
        enemiesRemaining--;

        // Recalculate current health from all alive enemies
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

        float totalCurrentHealth = 0f;
        foreach (EnemyHealth enemy in enemies)
        {
            totalCurrentHealth += enemy.currentHealth;
        }
        currentEnemyHealth = totalCurrentHealth;

        UpdateHealthBar();

        if (enemiesRemaining <= 0)
        {
            // Hide the entire health bar container (parent of the red bar)
            if (healthBar != null && healthBar.transform.parent != null)
                healthBar.transform.parent.gameObject.SetActive(false);

            if (healthText != null)
                healthText.gameObject.SetActive(false);
        }
    }

    private void UpdateHealthBar()
    {
        currentEnemyHealth = Mathf.Clamp(currentEnemyHealth, 0, totalEnemyHealth);

        float ratio = totalEnemyHealth > 0 ? (currentEnemyHealth / totalEnemyHealth) : 0f;
        float newWidth = ratio * barWidth;

        bool shouldShowBar = currentEnemyHealth > 0;

        if (healthBar != null)
        {
            healthBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);

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
        
        if (currentEnemyHealth <= 0)
        {
            Debug.LogError("Ending round");
            gameManager.EndRound();
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
            enemy.healthManager = this;
        }

        enemiesRemaining = newEnemies.Length;

        if (healthBar != null)
            healthBar.gameObject.SetActive(true);
        if (healthText != null)
            healthText.gameObject.SetActive(true);

        UpdateHealthBar();
    }
}
