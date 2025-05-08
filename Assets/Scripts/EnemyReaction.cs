using UnityEngine;
using UnityEngine.AI;

public class EnemyReaction : MonoBehaviour
{
    public NavMeshAgent agent;
    public float speedIncrease = 0.5f;
    public float maxSpeed = 6f;

    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    public void ReactToHit()
    {
        Debug.Log(gameObject.name + " was hit!");

        // Speed up logic
        if (agent != null)
        {
            agent.speed = Mathf.Min(agent.speed + speedIncrease, maxSpeed);
            Debug.Log("Speed increased to: " + agent.speed);
        }
    }
}
