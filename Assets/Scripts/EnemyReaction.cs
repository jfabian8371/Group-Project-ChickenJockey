using UnityEngine;
using UnityEngine.AI;

public class EnemyReaction : MonoBehaviour
{
    public Renderer enemyRenderer;
    public Color hitColor = Color.red;
    private Color originalColor;
    public float flashDuration = 0.1f;

    public NavMeshAgent agent;
    public float speedIncrease = 0.5f;
    public float maxSpeed = 6f;

    void Start()
    {
        // Get renderer
        if (enemyRenderer == null)
            enemyRenderer = GetComponentInChildren<Renderer>();

        if (enemyRenderer != null)
            originalColor = enemyRenderer.material.color;

        // Get NavMeshAgent
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    public void ReactToHit()
    {
        Debug.Log(gameObject.name + " was hit!");

        // Flash red
        if (enemyRenderer != null)
        {
            StopAllCoroutines();
            StartCoroutine(FlashOnHit());
        }

        // Speed up
        if (agent != null)
        {
            agent.speed = Mathf.Min(agent.speed + speedIncrease, maxSpeed);
            Debug.Log("Speed increased to: " + agent.speed);
        }
    }

    private System.Collections.IEnumerator FlashOnHit()
    {
        enemyRenderer.material.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        enemyRenderer.material.color = originalColor;
    }
}
