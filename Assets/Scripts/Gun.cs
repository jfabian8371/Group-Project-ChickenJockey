using UnityEngine;

public class GunShooter : MonoBehaviour
{
    public float range = 100f;
    public float damage = 10f;
    public Camera fpsCam;
    public AudioSource audioSource;
    public AudioClip fireSound;
    public GameObject impactEffectPrefab; // Assign your bullet impact prefab here

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // 🔊 Play firing sound
        if (fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSound);
        }

        // 🎯 Debug ray (visible in Scene view only)
        Debug.DrawRay(fpsCam.transform.position, fpsCam.transform.forward * range, Color.red, 1f);

        // 🔫 Raycast from screen center
        Ray ray = fpsCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            Debug.Log("Hit: " + hit.collider.name);

            // 💥 Create impact effect at hit point
            if (impactEffectPrefab != null)
            {
                GameObject impactGO = Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 1f); // Auto-destroy after 1 second
            }

            // Optional: Apply damage if hit object has a health script
            // var health = hit.collider.GetComponent<EnemyHealth>();
            // if (health != null)
            // {
            //     health.TakeDamage(damage);
            // }
        }
    }
}
