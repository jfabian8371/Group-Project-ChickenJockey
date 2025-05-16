using UnityEngine;

public class PlayerDamage : MonoBehaviour
{
    private PlayerHealth playerHealth;

    [Header("Damage Values")]
    public int bulletDamage = 10;
    public int contactDamage = 15;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth component not found on the player!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Handle bullet damage
        if (other.CompareTag("EnemyBullet"))
        {
            playerHealth?.TakeDamage(bulletDamage);
            Debug.Log("Player hit by bullet!");
            Destroy(other.gameObject);
        }

        // Handle enemy body collision damage
        if (other.CompareTag("Enemy"))
        {
            playerHealth?.TakeDamage(contactDamage);
            Debug.Log("Player collided with enemy!");
        }
    }
}
