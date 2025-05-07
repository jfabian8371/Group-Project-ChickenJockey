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
        // Play sound
        if (audioSource && swingSound)
            audioSource.PlayOneShot(swingSound);

        // Raycast in front of camera to simulate hit
        Ray ray = fpsCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            Debug.Log("Axe hit: " + hit.collider.name);

            if (hitEffect)
            {
                GameObject effect = Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(effect, 1f);
            }

            // Optional: Apply damage
            // var health = hit.collider.GetComponent<EnemyHealth>();
            // if (health != null) health.TakeDamage(damage);
        }
    }
}
