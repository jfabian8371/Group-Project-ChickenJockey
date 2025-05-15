using UnityEngine;
using System.Collections;

public class Rifle : MonoBehaviour, IWeapon
{
    public float range = 100f;
    public float damage = 10f;
    public float fireRate = 10f;
    public Camera fpsCam;
    public AudioSource audioSource;
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public GameObject impactEffectPrefab;

    public bool IsReloading => isReloading;
    public int CurrentAmmo => currentAmmo;
    public int ClipSize => clipSize;

    public int clipSize = 30;
    private int currentAmmo;
    public float reloadTime = 2f;
    private bool isReloading = false;

    private float nextTimeToFire = 0f;

    void Start()
    {
        currentAmmo = clipSize;
    }

    void Update()
    {
        if (isReloading)
            return;

        // Manual reload or auto reload if empty
        if (currentAmmo <= 0 || Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");

        if (reloadSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = clipSize;
        isReloading = false;
    }

    void Shoot()
    {
        if (currentAmmo <= 0)
            return;

        currentAmmo--;

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
