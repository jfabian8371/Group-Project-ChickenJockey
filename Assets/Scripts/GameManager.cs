using UnityEngine;
using StarterAssets; // For FirstPersonController
// using UnityEngine.SceneManagement; // If you need to reload scene on death, etc.

public class GameManager : MonoBehaviour
{
    [Header("Core References")]
    public PlayerHealth playerHealth;
    public FirstPersonController playerController; // Assign the player GameObject
    public WallController wallController;
    public WeaponSwitcher weaponSwitcher; // Assign the GameObject holding WeaponSwitcher

    // We'll get the active gun dynamically, but you might have a default or always-present one
    // public Gun activeGun; // If you have a primary gun always available for upgrades

    [Header("Game State")]
    public int currentRound = 0;
    public bool isUpgradeWallActive = false; // To prevent multiple triggers

    // For testing - manually trigger upgrade wall
    [Tooltip("Set to true in Inspector to show upgrade wall on next Update cycle (for testing)")]
    public bool triggerUpgradeWallManually = false;

    void Awake()
    {
        // Singleton pattern or service locator might be good here for easier access,
        // but for now, we'll rely on direct assignments.
        if (!playerHealth) Debug.LogError("PlayerHealth not assigned to GameManager!", this);
        if (!playerController) Debug.LogError("FirstPersonController not assigned to GameManager!", this);
        if (!wallController) Debug.LogError("WallController not assigned to GameManager!", this);
        if (!weaponSwitcher) Debug.LogError("WeaponSwitcher not assigned to GameManager!", this);

        if (wallController && playerHealth)
        {
            wallController.InitializeController(this, playerHealth);
        }
        else
        {
            Debug.LogError("Cannot initialize WallController due to missing PlayerHealth or WallController reference.", this);
        }
    }

    void Start()
    {
        // Initial game setup
        // For now, let's assume the game starts, and after a delay, the first round ends.
        // In a real game, you'd have gameplay leading to EndRound().
        StartNextRound(); // Start with round 0 or 1 logic
    }

    void Update()
    {
        // Manual trigger for testing
        if (triggerUpgradeWallManually)
        {
            triggerUpgradeWallManually = false; // Reset flag
            if (!isUpgradeWallActive)
            {
                EndRound(); // Simulate end of round to show wall
            }
        }

        // Example: Listen for player death
        // if (playerHealth != null && playerHealth.CurrentHealth <= 0)
        // {
        // HandleGameOver();
        // }
    }

    public void EndRound()
    {
        if (isUpgradeWallActive)
        {
            Debug.LogWarning("EndRound called, but upgrade wall is already active or being processed.");
            return;
        }

        Debug.Log($"Round {currentRound} ended.");
        isUpgradeWallActive = true; // Set flag to prevent re-triggering while it's showing
        ShowUpgradeWall();
    }

    void ShowUpgradeWall()
    {
        if (!playerHealth || !wallController)
        {
            Debug.LogError("Cannot show upgrade wall: PlayerHealth or WallController missing.", this);
            isUpgradeWallActive = false; // Reset flag if we can't show
            return;
        }

        int picks = 2;
        Debug.Log($"Player is at full health: {playerHealth.IsAtFullHealth()}. Allowed picks: {picks}");
        wallController.ShowWallAndPrepareUpgrades(picks);
    }

    public void StartNextRound()
    {
        isUpgradeWallActive = false; // Reset flag when new round starts
        currentRound++;
        Debug.Log($"Starting Round {currentRound}");

        // --- Placeholder for your round start logic ---
        // e.g., spawn enemies, reset player position, etc.

        // For testing, let's simulate playing a round and then ending it
        if (currentRound > 0) // Don't auto-end before the first round starts properly
        {
            Debug.Log("Simulating round play for 5 seconds...");
            // Simulate taking some damage to test health logic for upgrade picks
            /*if (playerHealth != null)
            {
                if (currentRound % 3 == 0 && playerHealth.CurrentHealth > 20)
                {
                    playerHealth.TakeDamage(30);
                }
                else if (currentRound % 2 == 0 && !playerHealth.IsAtFullHealth())
                {
                    playerHealth.Heal(25);
                }
            }*/
            Invoke(nameof(EndRound), 5f); // End round after 5 seconds for testing
        }
    }

    public void ApplyUpgrade(UpgradeDefinition upgrade)
    {
        if (upgrade == null)
        {
            Debug.LogError("ApplyUpgrade called with a null upgrade.", this);
            return;
        }

        Debug.Log($"GameManager applying upgrade: {upgrade.formattedDisplayName} (Type: {upgrade.type}, Value: {upgrade.actualValue})");

        // --- Apply the upgrade based on its type ---
        switch (upgrade.type)
        {
            case UpgradeType.HealToFull:
                if (playerHealth) playerHealth.HealToFull();
                break;

            case UpgradeType.IncreaseMaxHealth:
                if (playerHealth) playerHealth.IncreaseMaxHealth(upgrade.actualValue);
                break;

            case UpgradeType.DamageReduction:
                // actualValue is the percentage (e.g., 5 for 5%), so convert to 0.0 - 1.0 range
                if (playerHealth) playerHealth.IncreaseDamageReduction(upgrade.actualValue / 100f);
                break;

            case UpgradeType.MovementSpeed:
                // actualValue is the percentage (e.g., 7 for 7%), so convert to 0.0 - 1.0 range
                if (playerController) playerController.IncreaseMovementSpeed(upgrade.actualValue / 100f);
                break;

            case UpgradeType.Damage: // This currently assumes "Gun Damage"
                // We need to find the currently active Gun script.
                // This is a simplified approach. A more robust system might involve an event
                // or a dedicated "PlayerUpgrades" component that weapons subscribe to.
                GameObject currentWeaponObject = GetActiveWeapon();
                if (currentWeaponObject != null)
                {
                    Gun gun = currentWeaponObject.GetComponent<Gun>();
                    if (gun != null && upgrade.upgradeID.Contains("gun")) // Check if it's a gun-specific damage upgrade
                    {
                        gun.IncreaseDamage(upgrade.actualValue / 100f);
                    }
                    // else if (currentWeaponObject.GetComponent<Rifle>() != null && upgrade.upgradeID.Contains("rifle")) { /* Apply to Rifle */ }
                    else
                    {
                        Debug.LogWarning($"Damage upgrade selected, but active weapon '{currentWeaponObject.name}' is not a Gun or ID mismatch.", this);
                    }
                }
                else
                {
                    Debug.LogWarning("Damage upgrade selected, but no active weapon found to apply it to.", this);
                }
                break;

            case UpgradeType.FireRate: // This currently assumes "Gun Fire Rate"
                GameObject activeWeaponForFireRate = GetActiveWeapon();
                if (activeWeaponForFireRate != null)
                {
                    Gun gun = activeWeaponForFireRate.GetComponent<Gun>();
                    if (gun != null && upgrade.upgradeID.Contains("gun")) // Check if it's a gun-specific fire rate upgrade
                    {
                        gun.IncreaseFireRate(upgrade.actualValue / 100f);
                    }
                    // else if (activeWeaponForFireRate.GetComponent<Rifle>() != null && upgrade.upgradeID.Contains("rifle")) { /* Apply to Rifle */ }
                    else
                    {
                        Debug.LogWarning($"Fire Rate upgrade selected, but active weapon '{activeWeaponForFireRate.name}' is not a Gun or ID mismatch.", this);
                    }
                }
                else
                {
                    Debug.LogWarning("Fire Rate upgrade selected, but no active weapon found to apply it to.", this);
                }
                break;

            default:
                Debug.LogWarning($"Upgrade type '{upgrade.type}' not handled yet.", this);
                break;
        }
    }

    // In GameManager.cs
    private GameObject GetActiveWeapon()
    {
        if (weaponSwitcher == null)
        {
            // This case is likely fine since you said it's assigned.
            Debug.LogWarning("WeaponSwitcher not assigned to GameManager. Cannot get active weapon.", this);
            return null;
        }

        Debug.Log($"GetActiveWeapon: Attempting to find active weapon. WeaponSwitcher has {weaponSwitcher.weapons.Length} weapon slots.");
        for (int i = 0; i < weaponSwitcher.weapons.Length; i++)
        {
            GameObject weaponGO = weaponSwitcher.weapons[i]; // Renamed for clarity in logs

            if (weaponGO != null)
            {
                // Log the weapon's name and its activeSelf and activeInHierarchy states
                Debug.Log($"GetActiveWeapon: Checking slot {i}: '{weaponGO.name}'. " +
                          $"Is activeSelf? {weaponGO.activeSelf}. " +
                          $"Is activeInHierarchy? {weaponGO.activeInHierarchy}");

                if (weaponGO.activeInHierarchy)
                {
                    Debug.Log($"GetActiveWeapon: Found active weapon in slot {i}: '{weaponGO.name}'");
                    return weaponGO;
                }
            }
            else
            {
                Debug.LogWarning($"GetActiveWeapon: Weapon in slot {i} of WeaponSwitcher is null.");
            }
        }

        Debug.LogWarning("GetActiveWeapon: Finished loop. No weapon in WeaponSwitcher's list was found to be activeInHierarchy.", this);
        return null; // No weapon active
    }

    /*private GameObject GetActiveWeapon()
    {
        if (weaponSwitcher == null)
        {
            Debug.LogWarning("WeaponSwitcher not assigned to GameManager. Cannot get active weapon.", this);
            return null;
        }

        for (int i = 0; i < weaponSwitcher.weapons.Length; i++)
        {
            if (weaponSwitcher.weapons[i] != null && weaponSwitcher.weapons[i].activeInHierarchy)
            {
                return weaponSwitcher.weapons[i];
            }
        }
        return null; // No weapon active
    }*/

    // --- Test methods for UI buttons (optional, from previous version) ---
    // public void Test_EndRoundButton() { EndRound(); }
    // public void Test_TakeDamageButton() { if(playerHealth) playerHealth.TakeDamage(25); }
    // public void Test_HealButton() { if(playerHealth) playerHealth.Heal(50); }
}