using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealthManager : MonoBehaviour
{
    public Slider healthBar;
    public TMP_Text healthText; // Add this
    public float totalEnemyHealth = 0f;
    public float currentEnemyHealth = 0f;

    private void Start()
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

        foreach (EnemyHealth enemy in enemies)
        {
            totalEnemyHealth += enemy.maxHealth;
            currentEnemyHealth += enemy.currentHealth;
            enemy.manager = this;
        }

        UpdateHealthBar();
    }

    public void ReportDamage(float amount)
    {
        currentEnemyHealth -= amount;
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = currentEnemyHealth / totalEnemyHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.RoundToInt(currentEnemyHealth)}/{Mathf.RoundToInt(totalEnemyHealth)}";
        }
    }
}
