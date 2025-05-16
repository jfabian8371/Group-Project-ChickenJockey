using UnityEngine;

public class SniperBullet : MonoBehaviour
{
    public float lifetime = 10f;
    public float speed = 10f;
    public Transform target;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (target == null) return;

        // Move towards the target
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Rotate to face target
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
    }

    void OnTriggerEnter(Collider other)
    {
        // If the bullet hits the player
        if (other.CompareTag("Player"))
        {
            // Try to get the PlayerDamage component
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(10f); // Or whatever damage value you want
            }

            Destroy(gameObject);
        }
        else if (other.transform == target)
        {
            // If you want to keep original target logic
            Destroy(gameObject);
        }
    }

}
