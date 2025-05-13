using UnityEngine;

public class Rifle : MonoBehaviour
{
    public float range = 100f;
    public float damage = 10f;
    public float fireRate = 10f;
    public Camera fpsCam;
    public AudioSource audioSource;
    public AudioClip fireSound;
    public GameObject impactEffectPrefab;

    private float nextTimeToFire = 0f;

    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        if (fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSound);
        }

        Debug.DrawRay(fpsCam.transform.position, fpsCam.transform.forward * range, Color.green, 0.5f);

        Ray ray = fpsCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            Debug.Log("Rifle hit: " + hit.collider.name);

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
