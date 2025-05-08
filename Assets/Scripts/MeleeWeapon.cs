using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    public float range = 2f;
    public float damage = 25f;
    public float attackCooldown = 0.5f;
    public Camera fpsCam;
    public AudioSource audioSource;
    public AudioClip swingSound;
    public GameObject hitEffect;
    public Animator animator;

    private float nextAttackTime = 0f;

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            Attack();
        }
    }

    void Attack()
    {
        // 🔊 Play swing sound
        if (audioSource && swingSound)
            audioSource.PlayOneShot(swingSound);

        // 🎬 Trigger swing animation
        if (animator)
            animator.SetTrigger("Swing");

        // 🎯 Raycast to detect hit
        Ray ray = fpsCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            Debug.Log("Axe hit: " + hit.collider.name);

            // 💥 Spawn hit effect
            if (hitEffect)
            {
                GameObject effect = Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(effect, 1f);
            }

            // 🧠 Trigger enemy reaction
            EnemyReaction enemy = hit.collider.GetComponent<EnemyReaction>();
            if (enemy != null)
            {
                enemy.ReactToHit();
            }
        }
    }
}
