using UnityEngine;
using UnityEngine.AI; // For NavMeshAgent
using System.Collections; // Required for Invoke if we were using Coroutines, but Invoke is built-in

public class BearAIController : MonoBehaviour
{
    public Transform playerTarget;
    public float walkSpeed = 1.5f;
    public float runSpeed = 4.0f;
    public float sightRange = 15f;         // When it spots the player and decides to run
    public float attackRange = 2.5f;       // Range to initiate an attack
    public float attackCooldown = 3.0f;    // Time between attacks
    public float attackRecoveryDuration = 2.0f; // How long it stays in "Bear Idle O" conceptually

    [Header("Attack Settings")]
    public float attackDamage = 20f;
    public float attackHitBoxRange = 3.0f;
    public float attackHitAngle = 60f;
    public float attackDamageDelay = 0.2f; // <<< NEW: Time in seconds after attack starts to check for damage

    private NavMeshAgent agent;
    private Animator animator;
    private float lastAttackTime = -100f;
    private Vector3 lastKnownPlayerPosition;
    private bool isAttackingOrRecovering = false;
    private float recoveryTimer = 0f;

    private enum AIState
    {
        Walking,
        Chasing,
        Attacking,
        Recovering
    }
    private AIState currentState;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
            }
            else
            {
                Debug.LogError("Bear AI: Player target not set and not found by tag 'Player'!", this);
            }
        }

        currentState = AIState.Walking;
        if (agent != null && agent.isOnNavMesh)
        {
            agent.speed = walkSpeed;
        }
        else if (agent == null)
        {
            Debug.LogError("Bear AI: NavMeshAgent component not found!", this);
        }
        else if (!agent.isOnNavMesh)
        {
            Debug.LogError("Bear AI: NavMeshAgent is not on a NavMesh!", this);
        }
    }

    void Update()
    {
        if (playerTarget == null || agent == null || !agent.isOnNavMesh)
        {
            if (agent != null && agent.isOnNavMesh)
            {
                if (!agent.isStopped) agent.isStopped = true;
                if (agent.hasPath) agent.ResetPath();
                agent.velocity = Vector3.zero;
            }
            if (animator != null) animator.SetFloat("Speed", 0);
            return;
        }

        if (isAttackingOrRecovering)
        {
            HandleRecovery();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (animator != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude, 0.1f, Time.deltaTime);
        }

        switch (currentState)
        {
            case AIState.Walking:
                HandleWalkingState(distanceToPlayer);
                break;
            case AIState.Chasing:
                HandleChasingState(distanceToPlayer);
                break;
        }
    }

    void HandleWalkingState(float distanceToPlayer)
    {
        agent.speed = walkSpeed;
        agent.SetDestination(playerTarget.position);
        agent.isStopped = false;

        if (distanceToPlayer <= sightRange)
        {
            currentState = AIState.Chasing;
            lastKnownPlayerPosition = playerTarget.position;
            Debug.Log("Bear: Spotted player, switching to CHASE.");
        }
        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            InitiateAttack();
        }
    }

    void HandleChasingState(float distanceToPlayer)
    {
        agent.speed = runSpeed;
        agent.SetDestination(lastKnownPlayerPosition);
        agent.isStopped = false;

        if ((Vector3.Distance(transform.position, lastKnownPlayerPosition) <= agent.stoppingDistance + 0.5f || distanceToPlayer <= attackRange)
            && Time.time >= lastAttackTime + attackCooldown)
        {
            if (distanceToPlayer <= attackRange)
            {
                InitiateAttack();
            }
            else
            {
                Debug.Log("Bear: Reached last known spot, player not in attack range. Back to WALKING.");
                currentState = AIState.Walking;
            }
        }
        else if (distanceToPlayer > sightRange * 1.5f)
        {
            Debug.Log("Bear: Player too far, back to WALKING.");
            currentState = AIState.Walking;
        }
    }

    void InitiateAttack()
    {
        currentState = AIState.Attacking;
        isAttackingOrRecovering = true;

        if (agent.isOnNavMesh)
        {
            if (!agent.isStopped) agent.isStopped = true;
            if (agent.hasPath) agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0);
            if (playerTarget != null) transform.LookAt(playerTarget.position); // Ensure facing player
            animator.SetTrigger("Attack");
        }
        lastAttackTime = Time.time;
        Debug.Log("Bear: ATTACKING! Will attempt damage check in " + attackDamageDelay + " seconds.");

        // --- NEW: Call AttemptDamageAfterDelay using Invoke ---
        Invoke(nameof(AttemptDamageAfterDelay), attackDamageDelay); // Calls the function by its name after a delay

        recoveryTimer = attackRecoveryDuration;
    }

    void HandleRecovery()
    {
        if (agent.isOnNavMesh)
        {
            if (!agent.isStopped) agent.isStopped = true;
            if (agent.hasPath) agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
        if (animator != null) animator.SetFloat("Speed", 0);

        recoveryTimer -= Time.deltaTime;
        if (recoveryTimer <= 0)
        {
            isAttackingOrRecovering = false;
            currentState = AIState.Walking;
            if (agent.isOnNavMesh) agent.isStopped = false;
            Debug.Log("Bear: Recovery finished, back to WALKING.");
        }
    }

    // --- RENAMED & MODIFIED: This method is now called by Invoke, NOT an Animation Event ---
    void AttemptDamageAfterDelay() // Renamed from OnAttackAnimationHitMoment
    {
        // Check if still in an attacking state or if player became null during the delay
        if (!isAttackingOrRecovering || playerTarget == null)
        {
            Debug.LogWarning("Bear: AttemptDamageAfterDelay called, but no longer attacking or playerTarget is null. No damage dealt.");
            return;
        }

        //Debug.LogWarning("!!! BEAR: AttemptDamageAfterDelay FIRED (after " + attackDamageDelay + "s) !!!");

        // --- This is the "forced damage" version from your last script version ---
        // --- We will keep it for now to ensure the core damage dealing works ---
        // --- You can uncomment the range/angle checks later if this part works ---

        PlayerHealthManager playerHealthManager = playerTarget.GetComponent<PlayerHealthManager>();

        if (playerHealthManager == null)
        {
            Debug.LogError($"!!! BEAR: PlayerHealthManager NOT FOUND on {playerTarget.name} in AttemptDamageAfterDelay !!!");
            GameObject currentTarget = playerTarget.gameObject;
            PlayerHealth directPlayerHealth = currentTarget.GetComponent<PlayerHealth>();
            if (directPlayerHealth == null)
            {
                Debug.LogError($"!!! BEAR: Also, PlayerHealth script NOT FOUND on {playerTarget.name} !!!");
            }
            else
            {
                Debug.LogWarning($"BEAR: Found PlayerHealth script on {playerTarget.name}, but not PlayerHealthManager.");
            }
            return;
        }

        Debug.Log($"BEAR: Found PlayerHealthManager on {playerTarget.name}. Attempting to deal {attackDamage} damage via timed delay.");
        playerHealthManager.TakeDamage(attackDamage);
        Debug.Log($"BEAR: Called TakeDamage on PlayerHealthManager (timed delay). Check player health and console.");


        // --- ORIGINAL RANGE/ANGLE CHECKS (keep commented for now, uncomment when ready) ---
        /*
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        Debug.Log($"--- (Timed Delay) Hit Check --- Distance: {distanceToPlayer:F2} (Range: {attackHitBoxRange:F2})");

        if (distanceToPlayer <= attackHitBoxRange)
        {
            // It's a good idea to re-check LookAt if the player or bear might have moved during the delay
            if (playerTarget != null) transform.LookAt(playerTarget.position);

            Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToPlayer);
            Debug.Log($"--- (Timed Delay) Hit Check --- Angle: {angle:F2} (Max Angle: {attackHitAngle:F2})");

            if (angle <= attackHitAngle)
            {
                // PlayerHealthManager playerHealthManager = playerTarget.GetComponent<PlayerHealthManager>(); // Already got this
                if (playerHealthManager != null)
                {
                    Debug.Log($"Bear: SUCCESS (Timed Delay)! Dealing {attackDamage} damage to {playerTarget.name}");
                    playerHealthManager.TakeDamage(attackDamage);
                }
            }
            else
            {
                Debug.Log($"Bear: Attack missed (Timed Delay) {playerTarget.name} - Out of angle.");
            }
        }
        else
        {
            Debug.Log($"Bear: Attack missed (Timed Delay) {playerTarget.name} - Out of range.");
        }
        */
    }

    // OnAttackAnimationEnd can remain. It's not directly related to damage dealing in this approach.
    public void OnAttackAnimationEnd()
    {
        Debug.Log("Bear: Strike animation clip finished (actual anim clip). Recovery period continues via timer.");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (playerTarget != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, attackHitBoxRange);
            Vector3 forward = transform.forward;
            Quaternion leftRayRotation = Quaternion.AngleAxis(-attackHitAngle, Vector3.up);
            Quaternion rightRayRotation = Quaternion.AngleAxis(attackHitAngle, Vector3.up);
            Vector3 leftRayDirection = leftRayRotation * forward;
            Vector3 rightRayDirection = rightRayRotation * forward;
            Gizmos.DrawRay(transform.position, leftRayDirection * attackHitBoxRange);
            Gizmos.DrawRay(transform.position, rightRayDirection * attackHitBoxRange);
        }

        if (agent != null && agent.isOnNavMesh && agent.hasPath)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, agent.destination);
        }
    }
}