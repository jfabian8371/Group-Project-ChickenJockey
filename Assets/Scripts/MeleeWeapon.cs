using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    [Header("Attack Settings")]
    public float range = 2f;
    public float damage = 25f;
    public float attackCooldown = 0.5f;

    [Header("References")]
    public Camera fpsCam;
    public AudioSource audioSource;
    public AudioClip swingSound;
    public GameObject hitEffect;
    public Animator animator;

    private float nextAttackTime = 0f;

    void Update()
    {
        // Ensure this script only runs when the weapon is active in the hierarchy
        if (!gameObject.activeInHierarchy)
            return;

        // Check for attack input and cooldown
        if (Input.GetButtonDown("Fire1") && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            Attack();
        }
    }

    void Attack()
    {
        // 🔊 Play swing sound
        if (audioSource != null && swingSound != null)
        {
            audioSource.PlayOneShot(swingSound);
        }

        // 🎬 Trigger swing animation
        if (animator != null)
        {
            animator.SetTrigger("Swing");
        }

        // 🎯 Raycast from the center of the screen
        Ray ray = fpsCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            Debug.Log("Melee hit: " + hit.collider.name);

            // 💥 Spawn hit effect
            if (hitEffect != null)
            {
                GameObject effect = Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(effect, 1f);
            }

            // 🧠 React to hit if enemy
            EnemyReaction enemy = hit.collider.GetComponent<EnemyReaction>();
            if (enemy != null)
            {
                enemy.ReactToHit(); // Replace with enemy.TakeDamage(damage) if needed
            }
        }
    }

    void OnEnable()
    {
        Debug.Log($"{gameObject.name} equipped (melee weapon)");
    }

    void OnDisable()
    {
        Debug.Log($"{gameObject.name} unequipped (melee weapon)");
    }
}
