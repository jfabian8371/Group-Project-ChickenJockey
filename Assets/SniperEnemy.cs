using UnityEngine;
public class SniperEnemy : MonoBehaviour
{
    public Transform player;
    public GameObject sniperBulletPrefab;
    public Transform firePoint;

    public float bulletSpeed = 10f;
    public float shootInterval = 3f;
    public float retreatDistance = 25f;
    public float attackDistance = 40f;
    public float moveSpeed = 5f;

    private float shootTimer;
    private Animator animator;         // <-- NEW


    void Start()
    {
        animator = GetComponent<Animator>(); // Get the Animator on this GameObject
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        Vector3 previousPosition = transform.position;

        // Face the player
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 5f);

        // Movement logic
        bool isMoving = false;

        if (distance < retreatDistance)
        {
            // Retreat
            Vector3 moveDir = (transform.position - player.position).normalized;
            transform.position += moveDir * moveSpeed * Time.deltaTime;
            isMoving = true;
        }
        else if (distance > attackDistance)
        {
            // Approach
            Vector3 moveDir = (player.position - transform.position).normalized;
            transform.position += moveDir * moveSpeed * Time.deltaTime;
            isMoving = true;
        }

        // Set animation state
        if (animator)
        {
            animator.SetBool("isMoving", isMoving);
        }

        // Shooting logic
        if (distance <= attackDistance && distance >= retreatDistance)
        {
            
            shootTimer += Time.deltaTime;
            if (shootTimer >= shootInterval)
            {
                Shoot();
                shootTimer = 0f;
            }
        }
        else
        {
            shootTimer = 0f;
        }
    }

    void Shoot()
{
    if (sniperBulletPrefab && firePoint)
    {
        GameObject bullet = Instantiate(sniperBulletPrefab, firePoint.position, firePoint.rotation);

        // Set target on the bullet
        SniperBullet bulletScript = bullet.GetComponent<SniperBullet>();
        if (bulletScript != null && player != null)
        {
            // Try to find a child transform like "Armature" or "Head"
            Transform aimTarget = player.Find("Armature"); // Change this to match your actual rig

            // Fallback if not found
            if (aimTarget == null)
                aimTarget = player;

            bulletScript.target = aimTarget;
        }

        // Optional: give initial velocity (for appearance or physics effects)
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = firePoint.forward * bulletSpeed;
        }
    }
}

}
