using UnityEngine;
using StarterAssets; // For FirstPersonController

public class GameManager : MonoBehaviour
{
    [Header("Core References")]
    public PlayerHealth playerHealth;
    public FirstPersonController playerController;
    public WallController wallController;
    public WeaponSwitcher weaponSwitcher;
    public EnemySpawner enemySpawner;
    public GameObject roundStartText;
    public GameObject roundEndText;

    [Header("Global Player Modifiers")]
    [Tooltip("Current global damage multiplier. 1.0 = 100% base damage.")]
    public float globalDamageMultiplier = 1.0f;
    [Tooltip("Current global fire rate multiplier. 1.0 = 100% base fire rate.")]
    public float globalFireRateMultiplier = 1.0f;

    [Header("Game State")]
    public int currentRound = 0;
    public bool isUpgradeWallActive = false;

    [Tooltip("Set to true in Inspector to show upgrade wall on next Update cycle (for testing)")]
    public bool triggerUpgradeWallManually = false;

    void Awake()
    {
        if (!playerHealth) Debug.LogError("PlayerHealth not assigned to GameManager!", this);
        if (!playerController) Debug.LogError("FirstPersonController not assigned to GameManager!", this);
        if (!wallController) Debug.LogError("WallController not assigned to GameManager!", this);
        if (!weaponSwitcher) Debug.LogError("WeaponSwitcher not assigned to GameManager!", this);
        if (!enemySpawner) Debug.LogError("EnemySpawner not assigned to GameManager!", this);

        if (wallController && playerHealth)
        {
            wallController.InitializeController(this, playerHealth);
        }
        else
        {
            Debug.LogError("Cannot initialize WallController due to missing PlayerHealth or WallController reference.", this);
        }

        roundStartText.SetActive(false);
        roundEndText.SetActive(false);
    }

    void Start()
    {
        // Ensure all weapons are initialized with the starting global multipliers
        NotifyWeaponsOfStatChange();
        StartNextRound();
    }

    void Update()
    {
        if (triggerUpgradeWallManually)
        {
            triggerUpgradeWallManually = false;
            if (!isUpgradeWallActive)
            {
                //EndRound();
            }
        }
    }

    public void EndRound()
    {
        if (isUpgradeWallActive)
        {
            Debug.LogWarning("EndRound called, but upgrade wall is already active or being processed.");
            return;
        }

        roundEndText.SetActive(true);
        Invoke(nameof(DisableRoundEndText), 3f);

        Debug.Log($"Round {currentRound} ended.");
        isUpgradeWallActive = true;
        ShowUpgradeWall();
    }

    void ShowUpgradeWall()
    {
        if (!playerHealth || !wallController)
        {
            Debug.LogError("Cannot show upgrade wall: PlayerHealth or WallController missing.", this);
            isUpgradeWallActive = false;
            return;
        }
        int picks = 2; // Always 2 picks
        Debug.Log($"Allowed picks: {picks}");
        wallController.ShowWallAndPrepareUpgrades(picks);
    }

    private void DisableRoundStartText()
    {
        roundStartText.SetActive(false);
    }

    private void DisableRoundEndText()
    {
        roundEndText.SetActive(false);
    }

    public void StartNextRound()
    {
        isUpgradeWallActive = false;
        currentRound++;
        Debug.Log($"Starting Round {currentRound}");

        roundStartText.SetActive(true);
        Invoke(nameof(DisableRoundStartText), 3f);
        
        enemySpawner.SpawnWave();
        
        // increasing difficulty
        enemySpawner.numberOfEnemies += 5;

        // for debugging
        /*
        if (currentRound > 0)
        {
            Debug.Log($"Simulating round play for 5 seconds... (Round {currentRound})");
            Invoke(nameof(EndRound), 5f);
        }
        */
    }

    public void ApplyUpgrade(UpgradeDefinition upgrade)
    {
        if (upgrade == null)
        {
            Debug.LogError("ApplyUpgrade called with a null upgrade.", this);
            return;
        }

        Debug.Log($"GameManager applying upgrade: {upgrade.formattedDisplayName} (Type: {upgrade.type}, Value: {upgrade.actualValue})");

        switch (upgrade.type)
        {
            case UpgradeType.HealToFull:
                if (playerHealth) playerHealth.HealToFull();
                break;

            case UpgradeType.IncreaseMaxHealth:
                if (playerHealth) playerHealth.IncreaseMaxHealth(upgrade.actualValue);
                break;

            case UpgradeType.DamageReduction:
                if (playerHealth) playerHealth.IncreaseDamageReduction(upgrade.actualValue / 100f);
                break;

            case UpgradeType.MovementSpeed:
                if (playerController) playerController.IncreaseMovementSpeed(upgrade.actualValue / 100f);
                break;

            case UpgradeType.Damage: // Handles global damage
                // upgrade.actualValue is the percentage increase (e.g., 10 for +10%)
                globalDamageMultiplier += (upgrade.actualValue / 100f);
                Debug.Log($"Global Damage Multiplier is now: {globalDamageMultiplier:P1}"); // P1 for percentage with 1 decimal
                NotifyWeaponsOfStatChange(); // Tell all weapons to update their damage
                break;

            case UpgradeType.FireRate: // Handles global fire rate
                // upgrade.actualValue is the percentage increase (e.g., 15 for +15%)
                globalFireRateMultiplier += (upgrade.actualValue / 100f);
                Debug.Log($"Global Fire Rate Multiplier is now: {globalFireRateMultiplier:P1}");
                NotifyWeaponsOfStatChange(); // Tell all weapons to update their fire rate
                break;

            default:
                Debug.LogWarning($"Upgrade type '{upgrade.type}' not handled yet.", this);
                break;
        }
    }

    // NEW METHOD: Notifies all weapons in the WeaponSwitcher to update their stats
    public void NotifyWeaponsOfStatChange()
    {
        if (weaponSwitcher == null || weaponSwitcher.weapons == null)
        {
            Debug.LogWarning("WeaponSwitcher or its weapons array not available to notify stat changes.", this);
            return;
        }

        Debug.Log($"Notifying weapons of stat change: GlobalDmgMult={globalDamageMultiplier:P1}, GlobalFireRateMult={globalFireRateMultiplier:P1}");

        foreach (GameObject weaponGO in weaponSwitcher.weapons)
        {
            if (weaponGO == null) continue;

            // Try to get Gun component and update
            Gun gunComponent = weaponGO.GetComponent<Gun>();
            if (gunComponent != null)
            {
                gunComponent.UpdateStatsFromGlobal(globalDamageMultiplier, globalFireRateMultiplier);
            }

            // Try to get Rifle component and update
            Rifle rifleComponent = weaponGO.GetComponent<Rifle>();
            if (rifleComponent != null)
            {
                rifleComponent.UpdateStatsFromGlobal(globalDamageMultiplier, globalFireRateMultiplier);
            }

            // Add similar blocks here for any other weapon type scripts you create
            // e.g., Shotgun shotgunComponent = weaponGO.GetComponent<Shotgun>();
            //      if (shotgunComponent != null) { shotgunComponent.UpdateStatsFromGlobal(...); }
        }
    }

    // GetActiveWeapon is no longer used by ApplyUpgrade for Damage/FireRate
    // but might be useful for other game logic if you need to know the current weapon.
    // I've kept your version with more detailed logging.
    private GameObject GetActiveWeapon()
    {
        if (weaponSwitcher == null)
        {
            Debug.LogWarning("WeaponSwitcher not assigned to GameManager. Cannot get active weapon.", this);
            return null;
        }
        Debug.Log($"GetActiveWeapon: Attempting to find active weapon. WeaponSwitcher has {weaponSwitcher.weapons.Length} weapon slots.");
        for (int i = 0; i < weaponSwitcher.weapons.Length; i++)
        {
            GameObject weaponGO = weaponSwitcher.weapons[i];
            if (weaponGO != null)
            {
                Debug.Log($"GetActiveWeapon: Checking slot {i}: '{weaponGO.name}'. Is activeSelf? {weaponGO.activeSelf}. Is activeInHierarchy? {weaponGO.activeInHierarchy}");
                if (weaponGO.activeInHierarchy)
                {
                    Debug.Log($"GetActiveWeapon: Found active weapon in slot {i}: '{weaponGO.name}'");
                    return weaponGO;
                }
            }
            else Debug.LogWarning($"GetActiveWeapon: Weapon in slot {i} of WeaponSwitcher is null.");
        }
        Debug.LogWarning("GetActiveWeapon: Finished loop. No weapon in WeaponSwitcher's list was found to be activeInHierarchy.", this);
        return null;
    }
}