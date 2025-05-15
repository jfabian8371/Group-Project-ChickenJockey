using UnityEngine;
using System.Collections; // Required for IEnumerator (Reload)

public class Gun : MonoBehaviour, IWeapon
{
    [Header("Base Stats")]
    public float range = 100f;
    public float baseDamage = 10f;
    public float baseFireCooldown = 0.25f; // Time between shots
    public int clipSize = 10;
    public float reloadTime = 1.5f;

    [Header("Runtime Stats (Calculated)")]
    [SerializeField] private float currentDamage;
    [SerializeField] private float currentFireCooldown;
    [SerializeField] private int currentAmmo; // Made SerializeField to see in Inspector if desired
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
    private float nextFireTime = 0f;

    // REMOVED: Local multipliers are no longer needed for global system
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
        currentFireCooldown = baseFireCooldown;
        currentAmmo = clipSize; // Also initialize ammo here
    }

    // REMOVED: InitializeStats() as its logic is now split between Awake and UpdateStatsFromGlobal

    // void Start() // currentAmmo is now initialized in Awake
    // {
    // }

    void Update()
    {
        if (isReloading)
            return;

        // Auto-reload if empty, or manual reload
        if (currentAmmo <= 0 && !isReloading) // Ensure not already reloading
        {
            StartCoroutine(Reload());
            return;
        }
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < clipSize && !isReloading) // Manual reload only if not full and not already reloading
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime && currentAmmo > 0 && !isReloading)
        {
            nextFireTime = Time.time + currentFireCooldown; // Use the potentially modified currentFireCooldown
            Shoot();
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log($"{gameObject.name} Reloading..."); // Added game object name for clarity

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
        // No need to check currentAmmo <= 0 here again as Update does it before calling Shoot
        currentAmmo--;

        if (fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSound);
        }

        Debug.DrawRay(fpsCam.transform.position, fpsCam.transform.forward * range, Color.cyan, 0.5f);

        Ray ray = fpsCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            bool hitUpgradeChoice = false;
            if (!string.IsNullOrEmpty(upgradeChoiceTag) && hit.collider.CompareTag(upgradeChoiceTag))
            {
                UpgradeChoiceActivator choiceActivator = hit.collider.GetComponent<UpgradeChoiceActivator>();
                if (choiceActivator != null)
                {
                    Debug.Log("Gun hit an Upgrade Choice: " + hit.collider.name);
                    choiceActivator.ActivateByShooting();
                    hitUpgradeChoice = true;
                }
            }

            if (!hitUpgradeChoice)
            {
                Debug.Log("Hit: " + hit.collider.name + " with damage: " + currentDamage); // Use currentDamage

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

        // For fire cooldown, a higher global fire rate multiplier means a SHORTER cooldown
        if (globalFireRateMultiplier > 0) // Prevent division by zero
        {
            currentFireCooldown = baseFireCooldown / globalFireRateMultiplier;
        }
        else
        {
            currentFireCooldown = baseFireCooldown; // Fallback if multiplier is invalid
        }

        // Ensure cooldown doesn't become too small (or negative)
        if (currentFireCooldown < 0.01f)
        {
            currentFireCooldown = 0.01f;
        }

        Debug.Log($"{gameObject.name} stats updated from global: " +
                  $"Damage={currentDamage} (Base:{baseDamage} * GlobalMult:{globalDamageMultiplier}), " +
                  $"FireCooldown={currentFireCooldown} (Base:{baseFireCooldown} / GlobalMult:{globalFireRateMultiplier})");
    }

    // REMOVED: These methods are no longer needed as upgrades are global
    // public void IncreaseDamage(float percentageIncrease) { ... }
    // public void IncreaseFireRate(float percentageIncrease) { ... }
}