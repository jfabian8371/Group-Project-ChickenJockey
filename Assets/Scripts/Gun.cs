// Gun.cs
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Base Stats")]
    public float range = 100f;
    public float baseDamage = 10f;
    public float baseFireCooldown = 0.25f;

    [Header("Runtime Stats")]
    [SerializeField]
    private float currentDamage;
    [SerializeField]
    private float currentFireCooldown;

    [Header("References")]
    public Camera fpsCam;
    public AudioSource audioSource;
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public GameObject impactEffectPrefab;

    public float fireCooldown = 0.25f;

    [Header("Upgrade Interaction")]
    [Tooltip("Tag used on shootable upgrade choices.")]
    public string upgradeChoiceTag = "UpgradeChoice"; // Ensure this tag exists and is on your choice objects

    private float nextFireTime = 0f;
    private float _currentDamageMultiplier = 1.0f;
    private float _currentFireRateMultiplier = 1.0f;

    void Awake()
    {
        InitializeStats();
    }

    void InitializeStats()
    {
        currentDamage = baseDamage * _currentDamageMultiplier;
        currentFireCooldown = baseFireCooldown / _currentFireRateMultiplier;
        if (currentFireCooldown < 0.01f) currentFireCooldown = 0.01f;
    }

    public int clipSize = 10;
    private int currentAmmo;
    public float reloadTime = 1.5f;
    private bool isReloading = false;

    void Start()
    {
        currentAmmo = clipSize;
    }

    void Update()
    {
        if (isReloading)
            return;

        if (currentAmmo <= 0 || Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + currentFireCooldown;
            Shoot();
        }
    }

    System.Collections.IEnumerator Reload()
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

        Debug.DrawRay(fpsCam.transform.position, fpsCam.transform.forward * range, Color.cyan, 0.5f);

        Ray ray = fpsCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            // --- ADDED/MODIFIED SECTION FOR UPGRADE CHOICE INTERACTION ---
            bool hitUpgradeChoice = false;
            if (!string.IsNullOrEmpty(upgradeChoiceTag) && hit.collider.CompareTag(upgradeChoiceTag))
            {
                UpgradeChoiceActivator choiceActivator = hit.collider.GetComponent<UpgradeChoiceActivator>();
                if (choiceActivator != null)
                {
                    Debug.Log("Gun hit an Upgrade Choice: " + hit.collider.name);
                    choiceActivator.ActivateByShooting();
                    hitUpgradeChoice = true; // Mark that we hit an upgrade choice
                }
            }
            // --- END OF ADDED/MODIFIED SECTION ---

            // Only proceed with normal hit effects if we didn't hit an upgrade choice
            // OR if you want upgrade choices to also show impact effects (remove !hitUpgradeChoice)
            if (!hitUpgradeChoice)
            {
                Debug.Log("Hit: " + hit.collider.name + " with damage: " + currentDamage);

                if (impactEffectPrefab != null)
                {
                    // Ensure impact effect doesn't play on the upgrade choice itself if not desired
                    GameObject impactGO = Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impactGO, 1f);
                }

                // Assuming EnemyReaction will have a TakeDamage(float amount) method eventually
                EnemyReaction enemy = hit.collider.GetComponent<EnemyReaction>();
                if (enemy != null)
                {
                    // enemy.TakeDamage(currentDamage); // IDEAL
                    enemy.ReactToHit(); // Current
                }
            }
            else // Optional: if you want a different impact effect for upgrade choices
            {
                if (impactEffectPrefab != null) // You might want a specific "UI hit" effect
                {
                    GameObject impactGO = Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impactGO, 0.5f); // Shorter lifespan for UI hit maybe
                }
            }
        }
    }

    public void IncreaseDamage(float percentageIncrease)
    {
        if (percentageIncrease <= 0) return;
        _currentDamageMultiplier += percentageIncrease;
        currentDamage = baseDamage * _currentDamageMultiplier;
        Debug.Log($"{gameObject.name} damage increased by {percentageIncrease * 100}%. New Damage: {currentDamage}");
    }

    public void IncreaseFireRate(float percentageIncrease)
    {
        if (percentageIncrease <= 0) return;
        _currentFireRateMultiplier += percentageIncrease;
        currentFireCooldown = baseFireCooldown / _currentFireRateMultiplier;
        if (currentFireCooldown < 0.01f) currentFireCooldown = 0.01f;
        Debug.Log($"{gameObject.name} fire rate increased by {percentageIncrease * 100}%. New Cooldown: {currentFireCooldown}");
    }
}