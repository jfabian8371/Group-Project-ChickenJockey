using UnityEngine;
using UnityEngine.AI;
using System.Collections; // Required for Coroutines

public class GhostAIController : MonoBehaviour
{
    public Transform player; // Assign your Player GameObject/Transform here in the Inspector
    private NavMeshAgent agent;

    // --- Damage Properties ---
    [Header("Contact Damage Settings")]
    public float damageAmount = 10f;        // How much damage to deal on contact
    public float damageAndPauseCooldown = 3.0f; // Total cooldown before ghost can attempt damage AND move again (was damageCooldown)
                                                // This now includes the pause duration.
    public float pauseDurationAfterDamage = 2.0f; // How long the ghost stops moving after dealing damage

    private float lastDamageCycleTime = -100f;   // Time when the last damage cycle (damage + pause) began
    private bool isPausedAfterDamage = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("GhostAIController: NavMeshAgent component not found!", this);
            enabled = false;
            return;
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("GhostAIController: Player found by tag.", this);
            }
            else
            {
                Debug.LogError("GhostAIController: Player target not assigned and not found by tag 'Player'!", this);
                enabled = false;
            }
        }
    }

    void Update()
    {
        if (player == null || agent == null || !agent.isOnNavMesh)
        {
            if (agent != null && agent.isOnNavMesh && !agent.isStopped) agent.isStopped = true;
            return;
        }

        // If paused after damage, don't update destination
        if (isPausedAfterDamage)
        {
            // The agent is already stopped by the Coroutine
            return;
        }

        // If enough time has passed since the last full damage cycle, allow movement
        if (Time.time >= lastDamageCycleTime + damageAndPauseCooldown)
        {
            if (agent.isStopped)
            {
                agent.isStopped = false; // Resume movement if it was stopped
            }
            agent.SetDestination(player.position);
        }
        else
        {
            // Still in cooldown from previous damage/pause cycle, but not actively paused
            // (meaning the 2s pause is over, but the 3s total cooldown isn't)
            // Ensure it's stopped if it's not supposed to be moving yet due to overall cooldown
            if (!agent.isStopped)
            {
                agent.isStopped = true;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the object is the player, if not currently paused,
        // and if enough time has passed since the last full damage/pause cycle
        if (!isPausedAfterDamage && other.CompareTag("Player") && Time.time >= lastDamageCycleTime + damageAndPauseCooldown)
        {
            Debug.Log($"Ghost: Touched Player '{other.name}'. Attempting damage and pause.");

            PlayerHealthManager playerHealthManager = other.GetComponent<PlayerHealthManager>();
            if (playerHealthManager != null)
            {
                playerHealthManager.TakeDamage(damageAmount);
                lastDamageCycleTime = Time.time; // Mark the beginning of this damage/pause cycle
                Debug.Log($"Ghost: Dealt {damageAmount} damage to {other.name}. Starting pause for {pauseDurationAfterDamage}s.");
                StartCoroutine(PauseAfterDamageRoutine());
            }
            else
            {
                Debug.LogWarning($"Ghost: Touched {other.name} tagged as Player, but it has no PlayerHealthManager script.", this);
            }
        }
    }

    IEnumerator PauseAfterDamageRoutine()
    {
        isPausedAfterDamage = true;
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true; // Stop NavMeshAgent movement
            agent.velocity = Vector3.zero; // Ensure it's not sliding
            if (agent.hasPath) agent.ResetPath();
        }

        yield return new WaitForSeconds(pauseDurationAfterDamage);

        isPausedAfterDamage = false;
        // The Update loop will handle resuming movement based on the damageAndPauseCooldown
        // If the total cooldown is met, it will allow movement.
        // If not, it will remain stopped until the cooldown is over.
        Debug.Log("Ghost: Pause finished. Will resume movement when cooldown allows.");
    }

}
