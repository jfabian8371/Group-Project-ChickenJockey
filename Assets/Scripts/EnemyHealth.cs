using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    public EnemyHealthManager healthManager;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            TakeDamage(1f);
        }
    }

    // Call this when you want to damage the enemy
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        healthManager.ReportDamage(amount);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (healthManager != null)
        {
            healthManager.ReportEnemyDeath(currentHealth);
        }

        Destroy(gameObject);
    }

}
