using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 10f;
    public float currentHealth;

    public EnemyHealthManager manager;

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

        manager.ReportDamage(amount);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (manager != null)
        {
            manager.ReportEnemyDeath(currentHealth);
        }

        Destroy(gameObject);
    }

}
