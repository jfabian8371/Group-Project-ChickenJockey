using UnityEngine;
using UnityEngine.AI;

public class GhostEnemyReaction : MonoBehaviour
{
    public NavMeshAgent agent;
    public float speedIncrease = 0.5f;
    public float maxSpeed = 6f;

    public AudioSource audioSource;
    public AudioClip hitSound;

    public float scaleIncrease = 0.1f;   // How much bigger it gets each time
    public float maxScale = 3f;          // Maximum size allowed

    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void ReactToHit()
    {
        Debug.Log(gameObject.name + " was hit!");

        // 🔊 Play hit sound
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // 🚀 Speed up
        if (agent != null)
        {
            agent.speed = Mathf.Min(agent.speed + speedIncrease, maxSpeed);
            Debug.Log("Speed increased to: " + agent.speed);
        }

        // 🔁 Scale up (but cap it)
        Vector3 newScale = transform.localScale + Vector3.one * scaleIncrease;
        if (newScale.x <= maxScale)
        {
            transform.localScale = newScale;
        }
    }
}
