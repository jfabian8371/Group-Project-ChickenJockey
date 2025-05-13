using UnityEngine;

public class Gun : MonoBehaviour
{
    public float range = 100f;
    public float damage = 10f;
    public Camera fpsCam;
    public AudioSource audioSource;
    public AudioClip fireSound;
    public GameObject impactEffectPrefab;

    public float fireCooldown = 0.25f; // delay between shots (clicks)
    private float nextFireTime = 0f;

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireCooldown;
            Shoot();
        }
    }

    void Shoot()
    {
        if (fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSound);
        }

        Debug.DrawRay(fpsCam.transform.position, fpsCam.transform.forward * range, Color.cyan, 0.5f);

        Ray ray = fpsCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            Debug.Log("Hit: " + hit.collider.name);

            if (impactEffectPrefab != null)
            {
                GameObject impactGO = Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 1f);
            }

            EnemyReaction enemy = hit.collider.GetComponent<EnemyReaction>();
            if (enemy != null)
            {
                enemy.ReactToHit();
            }
        }
    }
}
