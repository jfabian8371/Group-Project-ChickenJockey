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
        // Optional: prevent hitting non-targets
        if (other.transform == target)
        {
            Debug.Log("Sniper bullet hit target: " + other.name);
            Destroy(gameObject);
        }
    }
}
