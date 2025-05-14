using UnityEngine;

public class SniperBullet : MonoBehaviour
{
    public float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        // Add damage or hit logic here
        Debug.Log("Sniper bullet hit: " + other.name);
        Destroy(gameObject);
    }
}
