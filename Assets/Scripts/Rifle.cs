using UnityEngine;
using System.Collections;

public class Rifle : MonoBehaviour, IWeapon
{
    [Header("Base Stats")]
    public float range = 100f;
    public float baseDamage = 10f;
    public float baseFireRate = 10f;     // Shots per second
    public int clipSize = 30;
    public float reloadTime = 2f;

    [Header("Runtime Stats (Calculated)")]
    [SerializeField] private float currentDamage;
    [SerializeField] private float currentFireRate;    // Actual shots per second after upgrades
    [SerializeField] private int currentAmmo;        // Made SerializeField
    [SerializeField] private bool isReloading = false; // Made SerializeField

    [Header("References")]
    public Camera fpsCam;
    public AudioSource audioSource;
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public GameObject impactEffectPrefab;

    [Header("Upgrade Interaction")]
    [Tooltip("Tag used on shootable upgrade choices.")]
    public string upgradeChoiceTag = "UpgradeChoice";

    // Private working variables
    private float nextTimeToFire = 0f;

    // REMOVED: Local multipliers
    // private float _currentDamageMultiplier = 1.0f;
    // private float _currentFireRateMultiplier = 1.0f;

    // Public properties for external access if needed
    public bool IsReloading => isReloading;
    public int CurrentAmmo => currentAmmo;
    public int ClipSize => clipSize;

    void Awake()
    {
        // Initialize current stats to base stats.
        // GameManager will call UpdateStatsFromGlobal to apply multipliers.
        currentDamage = baseDamage;
        currentFireRate = baseFireRate;
        currentAmmo = clipSize; // Initialize ammo in Awake
    }

    // REMOVED: InitializeStats() that used local multipliers

    // void Start() // currentAmmo now initialized in Awake
    // {
    // }

    void Update()
    {
        if (isReloading)
            return;

        if (currentAmmo <= 0 && !isReloading)
        {
            StartCoroutine(Reload());
            return;
        }
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < clipSize && !isReloading)
        {
            StartCoroutine(Reload());
            return;
        }

        // Automatic fire (holding button)
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire && currentAmmo > 0 && !isReloading)
        {
            if (currentFireRate > 0) // Use the potentially modified currentFireRate
            {
                nextTimeToFire = Time.time + 1f / currentFireRate;
            }
            else
            {
                nextTimeToFire = Time.time + 1f; // Fallback if fire rate is invalid
            }
            Shoot();
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log($"{gameObject.name} Reloading..."); // Added game object name

        if (reloadSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = clipSize;
        isReloading = false;
        Debug.Log($"{gameObject.name} Reload Finished. Ammo: {currentAmmo}");
    }

    void Shoot()
    {
        // No need to check currentAmmo <= 0 here again
        currentAmmo--;

        if (fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSound);
        }

        Debug.DrawRay(fpsCam.transform.position, fpsCam.transform.forward * range, Color.green, 0.5f);

        Ray ray = fpsCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            bool hitUpgradeChoice = false;
            if (!string.IsNullOrEmpty(upgradeChoiceTag) && hit.collider.CompareTag(upgradeChoiceTag))
            {
                UpgradeChoiceActivator choiceActivator = hit.collider.GetComponent<UpgradeChoiceActivator>();
                if (choiceActivator != null)
                {
                    Debug.Log("Rifle hit an Upgrade Choice: " + hit.collider.name);
                    choiceActivator.ActivateByShooting();
                    hitUpgradeChoice = true;
                }
            }

            if (!hitUpgradeChoice)
            {
                Debug.Log("Rifle hit: " + hit.collider.name + " with damage: " + currentDamage); // Use currentDamage

                if (impactEffectPrefab != null)
                {
                    GameObject impactGO = Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impactGO, 1f);
                }

                EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(currentDamage);
                }

                GhostEnemyReaction ghostEnemyReaction = hit.collider.GetComponent<GhostEnemyReaction>();
                if (ghostEnemyReaction != null)
                {
                    // enemy.TakeDamage(currentDamage);
                    ghostEnemyReaction.ReactToHit();
                }
            }
            else
            {
                if (impactEffectPrefab != null)
                {
                    GameObject impactGO = Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impactGO, 0.5f);
                }
            }
        }
    }

    // NEW METHOD: Called by GameManager to update stats based on global player multipliers
    public void UpdateStatsFromGlobal(float globalDamageMultiplier, float globalFireRateMultiplier)
    {
        currentDamage = baseDamage * globalDamageMultiplier;
        currentFireRate = baseFireRate * globalFireRateMultiplier; // Direct multiplication for shots per second

        // Ensure fire rate doesn't become too small (or negative)
        if (currentFireRate < 0.1f) // Minimum fire rate (e.g., 1 shot every 10 seconds)
        {
            currentFireRate = 0.1f;
        }

        Debug.Log($"{gameObject.name} stats updated from global: " +
                  $"Damage={currentDamage} (Base:{baseDamage} * GlobalMult:{globalDamageMultiplier}), " +
                  $"FireRate={currentFireRate} (Base:{baseFireRate} * GlobalMult:{globalFireRateMultiplier})");
    }

    // REMOVED: These methods are no longer needed
    // public void IncreaseDamage(float percentageIncrease) { ... }
    // public void IncreaseFireRate(float percentageIncrease) { ... }
}