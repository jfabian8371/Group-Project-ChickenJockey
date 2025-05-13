using UnityEngine;
using UnityEngine.AI; // For NavMeshAgent

public class BearAIController : MonoBehaviour
{
    public Transform playerTarget;
    public float walkSpeed = 1.5f;
    public float runSpeed = 4.0f;
    public float sightRange = 15f;         // When it spots the player and decides to run
    public float attackRange = 2.5f;       // Range to initiate an attack
    public float attackCooldown = 3.0f;    // Time between attacks
    public float attackRecoveryDuration = 2.0f; // How long it stays in "Bear Idle O" conceptually

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
            // Try to find player by tag if not assigned
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTarget = playerObj.transform;
            else Debug.LogError("Bear AI: Player target not set and not found by tag 'Player'!");
        }

        currentState = AIState.Walking; // Start by walking
        agent.speed = walkSpeed;
    }

    void Update()
    {
        if (playerTarget == null || isAttackingOrRecovering) // If no target or in attack/recovery animation sequence
        {
            if (isAttackingOrRecovering)
            {
                HandleRecovery();
            }
            else // No target
            {
                agent.isStopped = true;
                animator.SetFloat("Speed", 0);
            }
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        // Always update the animator's speed parameter based on agent's current velocity
        animator.SetFloat("Speed", agent.velocity.magnitude, 0.1f, Time.deltaTime);

        switch (currentState)
        {
            case AIState.Walking:
                HandleWalkingState(distanceToPlayer);
                break;
            case AIState.Chasing:
                HandleChasingState(distanceToPlayer);
                break;
                // Attacking and Recovering are handled via isAttackingOrRecovering flag and animation events/transitions
        }
    }

    void HandleWalkingState(float distanceToPlayer)
    {
        agent.speed = walkSpeed;
        agent.SetDestination(playerTarget.position); // Slowly walk towards current player position
        agent.isStopped = false;

        if (distanceToPlayer <= sightRange)
        {
            currentState = AIState.Chasing;
            lastKnownPlayerPosition = playerTarget.position; // Store where player was spotted
            Debug.Log("Bear: Spotted player, switching to CHASE.");
        }
        // Check if close enough to attack even from walk state
        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            InitiateAttack();
        }
    }

    void HandleChasingState(float distanceToPlayer)
    {
        agent.speed = runSpeed;
        agent.SetDestination(lastKnownPlayerPosition); // Run towards where player WAS spotted
        agent.isStopped = false;

        // If reached the last known position OR player is within attack range
        if ((Vector3.Distance(transform.position, lastKnownPlayerPosition) <= agent.stoppingDistance + 0.5f || distanceToPlayer <= attackRange)
            && Time.time >= lastAttackTime + attackCooldown)
        {
            if (distanceToPlayer <= attackRange) // Check if player is ACTUALLY in range
            {
                InitiateAttack();
            }
            else // Reached spot, player not there, go back to walking/searching
            {
                Debug.Log("Bear: Reached last known spot, player not in attack range. Back to WALKING.");
                currentState = AIState.Walking;
            }
        }
        else if (distanceToPlayer > sightRange * 1.5f) // Player got too far away
        {
            Debug.Log("Bear: Player too far, back to WALKING.");
            currentState = AIState.Walking;
        }
    }

    void InitiateAttack()
    {
        currentState = AIState.Attacking; // Conceptually
        isAttackingOrRecovering = true;

        agent.isStopped = true; // Stop moving to attack
        animator.SetFloat("Speed", 0); // Ensure speed is zero for attack anim
        transform.LookAt(playerTarget.position); // Face the player
        animator.SetTrigger("Attack");
        lastAttackTime = Time.time;
        Debug.Log("Bear: ATTACKING!");

        // The Animator will transition Bear_Strike2 -> Bear Idle O automatically.
        // We'll use HandleRecovery to manage the Bear Idle O phase.
        recoveryTimer = attackRecoveryDuration; // Start recovery countdown
    }

    void HandleRecovery()
    {
        // This method is called while isAttackingOrRecovering is true.
        // The actual animation is playing (Strike2 then Idle O).
        // We use a timer to decide when to exit the "conceptual" recovery state.

        // Ensure speed is 0 while in Bear Idle O (recovery anim)
        // The animator state machine should handle this visually if `Bear Idle O` is a static anim,
        // but it's good to ensure NavMeshAgent isn't trying to move.
        if (agent.isOnNavMesh) agent.isStopped = true;
        animator.SetFloat("Speed", 0);


        recoveryTimer -= Time.deltaTime;
        if (recoveryTimer <= 0)
        {
            isAttackingOrRecovering = false;
            currentState = AIState.Walking; // Reset to walking state
            agent.isStopped = false; // Allow movement again
            Debug.Log("Bear: Recovery finished, back to WALKING.");
        }
    }

    // Optional: Animation Event
    // You can add an Animation Event at the end of "Bear_Strike2" animation
    // to call a method if you need more precise timing for when the actual strike happens
    // or when recovery should strictly begin.
    public void OnAttackAnimationHitMoment() // Call this from animation event
    {
        Debug.Log("Bear: Attack Hit Frame!");
        // Deal damage here if your attack animation has a specific hit frame
    }
    public void OnAttackAnimationEnd() // Call from Anim Event at end of Bear_Strike2
    {
        // The transition to Bear Idle O is automatic.
        // The recoveryTimer logic already handles the duration of the recovery phase.
        // This function might be useful if you need to trigger something specific *exactly* when Strike2 animation clip finishes.
        Debug.Log("Bear: Strike animation clip finished, entering recovery animation (Bear Idle O).");
    }


    void OnDrawGizmosSelected()
    {
        // For visualizing ranges in the editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, agent.destination);
        }
    }
}