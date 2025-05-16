using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public EnemyHealthManager healthManager;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        healthManager?.ReportDamage(damage);

        if (currentHealth <= 0)
        {
            healthManager?.ReportEnemyDeath();
            Die();
        }
    }

    private void Die()
    {
        // Destroy or disable enemy
        Destroy(gameObject);
    }
}
