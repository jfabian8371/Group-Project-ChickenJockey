using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq; // For Shuffle, though simple swap is used currently

public class UpgradeUIManager : MonoBehaviour
{
    [Header("UI & Choice Elements")]
    [Tooltip("The parent GameObject that is shown/hidden (e.g., UpgradeWallObject)")]
    public GameObject upgradePanelContainer;
    public TextMeshProUGUI instructionText;

    [Tooltip("Assign the 4 GameObjects here that player will shoot. Each should have UpgradeChoiceActivator and a Collider.")]
    public UpgradeChoiceActivator[] shootableChoices = new UpgradeChoiceActivator[4];

    [Tooltip("Assign the 4 TextMeshProUGUI elements for displaying upgrade names/details, corresponding to shootableChoices.")]
    public TextMeshProUGUI[] choiceDisplayTexts = new TextMeshProUGUI[4];

    private List<UpgradeDefinition> allPossibleRandomUpgrades = new List<UpgradeDefinition>();
    private UpgradeDefinition healToFullUpgrade;

    private int picksAllowed;
    private int picksMade;

    private GameManager gameManager;
    private WallController wallController;
    private PlayerHealth playerHealth;

    void Awake()
    {
        if (shootableChoices.Length != 4 || choiceDisplayTexts.Length != 4)
        {
            Debug.LogError("UpgradeUIManager requires exactly 4 Shootable Choices and 4 Choice Display Texts assigned!");
            return;
        }

        InitializeUpgradeDefinitions(); // Call this in Awake
        if (upgradePanelContainer) upgradePanelContainer.SetActive(false);

        for (int i = 0; i < shootableChoices.Length; i++)
        {
            if (shootableChoices[i] != null)
            {
                shootableChoices[i].ResetChoice();
            }
            else
            {
                Debug.LogError($"Shootable Choice at index {i} is not assigned!", this);
            }
        }
    }

    void InitializeUpgradeDefinitions()
    {
        Debug.LogError("BBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        allPossibleRandomUpgrades.Clear(); // Clear before repopulating if called multiple times (though usually just once in Awake)

        // Heal to Full (always specific)
        healToFullUpgrade = new UpgradeDefinition(
            "heal_full",
            UpgradeType.HealToFull,
            "Heal to Full",
            0f // Value not used for rarity scaling
        );
        // Note: healToFullUpgrade is NOT added to allPossibleRandomUpgrades list

        // Player-specific upgrades
        allPossibleRandomUpgrades.Add(new UpgradeDefinition("max_health_boost", UpgradeType.IncreaseMaxHealth, "+{0} Max Health", 10f));
        allPossibleRandomUpgrades.Add(new UpgradeDefinition("dmg_reduction", UpgradeType.DamageReduction, "+{0}% Damage Reduction", 5f));
        allPossibleRandomUpgrades.Add(new UpgradeDefinition("move_speed_boost", UpgradeType.MovementSpeed, "+{0}% Movement Speed", 7f)); // Was 10%, example value change

        // --- MODIFIED FOR GLOBAL WEAPON UPGRADES ---
        // Generic damage and fire rate upgrades that apply to all weapons
        allPossibleRandomUpgrades.Add(new UpgradeDefinition(
            "global_damage_boost", // Generic ID
            UpgradeType.Damage,
            "+{0}% All Weapon Damage", // Updated display name
            10f // Base common value for the global damage %
        ));
        allPossibleRandomUpgrades.Add(new UpgradeDefinition(
            "global_fire_rate_boost", // Generic ID
            UpgradeType.FireRate,
            "+{0}% All Weapon Fire Rate", // Updated display name
            15f // Base common value for the global fire rate %
        ));
        // --- END OF MODIFICATION ---

        // REMOVE old weapon-specific upgrade definitions if they are now global:
        // allPossibleRandomUpgrades.RemoveAll(ud => ud.upgradeID == "gun_dmg_boost");
        // allPossibleRandomUpgrades.RemoveAll(ud => ud.upgradeID == "gun_fire_rate_boost");
        // (Or just don't add them in the first place as done above)
    }

    public void InitializeManagerReferences(GameManager gm, WallController wc, PlayerHealth ph)
    {
        gameManager = gm;
        wallController = wc;
        playerHealth = ph;
        if (!playerHealth) Debug.LogError("PlayerHealth not assigned to UpgradeUIManager!", this);
    }

    public void PrepareAndShowUpgradeScreen(int numberOfPicks)
    {
        if (!instructionText || shootableChoices.Length != 4 || !playerHealth || gameManager == null) // Added gameManager null check
        {
            Debug.LogError("Upgrade UI elements not fully assigned or core references (PlayerHealth/GameManager) missing!", this);
            if (wallController) wallController.HideWallAndProceed();
            return;
        }

        picksAllowed = numberOfPicks;
        picksMade = 0;
        instructionText.text = $"Pick {picksAllowed} upgrade(s)";

        foreach (UpgradeChoiceActivator choiceActivator in shootableChoices)
        {
            if (choiceActivator != null) choiceActivator.ResetChoice();
        }

        // Slot 0: Always Heal to Full
        if (healToFullUpgrade == null) Debug.LogError("AHHHHHHHHHHHHHHHHHHHHHHHHHHH");
        healToFullUpgrade.DetermineAndSetInstanceDetails(useRandomRarity: false); // No random rarity for heal
        SetupChoiceVisuals(shootableChoices[0], choiceDisplayTexts[0], healToFullUpgrade);

        // Slots 1, 2, 3: Random Upgrades
        List<UpgradeDefinition> tempAvailableUpgrades = new List<UpgradeDefinition>(allPossibleRandomUpgrades);

        if (playerHealth.IsDamageReductionCapped())
        {
            tempAvailableUpgrades.RemoveAll(ud => ud.type == UpgradeType.DamageReduction);
        }
        // Optional: Add similar cap checks for globalDamageMultiplier and globalFireRateMultiplier if you want to cap them
        // Example: if (gameManager.globalDamageMultiplier >= 2.0f) // If global damage multiplier reached 200% (base + 100% bonus)
        //          { tempAvailableUpgrades.RemoveAll(ud => ud.type == UpgradeType.Damage); }


        // Simple shuffle (Fisher-Yates is better for true randomness but this is often sufficient)
        for (int i = 0; i < tempAvailableUpgrades.Count - 1; i++)
        {
            int randomIndex = Random.Range(i, tempAvailableUpgrades.Count);
            // Swap
            (tempAvailableUpgrades[i], tempAvailableUpgrades[randomIndex]) = (tempAvailableUpgrades[randomIndex], tempAvailableUpgrades[i]);
        }

        for (int i = 1; i < 4; i++) // For choices 1, 2, and 3
        {
            if (shootableChoices[i] == null || choiceDisplayTexts[i] == null) continue;

            if (tempAvailableUpgrades.Count > (i - 1)) // i-1 because we're picking for slot 1, 2, 3 from the shuffled list
            {
                UpgradeDefinition sourceDefinition = tempAvailableUpgrades[i - 1];
                // Create a new instance for this specific display roll to hold its unique rarity/value
                UpgradeDefinition rolledUpgrade = new UpgradeDefinition(
                    sourceDefinition.upgradeID,
                    sourceDefinition.type,
                    sourceDefinition.baseDisplayName,
                    sourceDefinition.commonValue
                );
                rolledUpgrade.DetermineAndSetInstanceDetails(); // Roll rarity and set final values

                SetupChoiceVisuals(shootableChoices[i], choiceDisplayTexts[i], rolledUpgrade);
                shootableChoices[i].gameObject.SetActive(true);
            }
            else
            {
                shootableChoices[i].gameObject.SetActive(false);
                choiceDisplayTexts[i].text = ""; // Clear text if no upgrade for this slot
            }
        }
        if (upgradePanelContainer) upgradePanelContainer.SetActive(true);
    }

    void SetupChoiceVisuals(UpgradeChoiceActivator choiceActivator, TextMeshProUGUI textElement, UpgradeDefinition upgradeDef)
    {
        if (choiceActivator == null || textElement == null || upgradeDef == null)
        {
            Debug.LogError($"SetupChoiceVisuals: Missing element. ActivatorNull?{choiceActivator == null}, TextNull?{textElement == null}, DefNull?{upgradeDef == null}", choiceActivator?.gameObject);
            return;
        }
        choiceActivator.InitializeChoice(upgradeDef, this);
        textElement.text = upgradeDef.formattedDisplayName;

        Renderer choiceRenderer = choiceActivator.GetComponent<Renderer>();
        if (choiceRenderer != null)
        {
            choiceRenderer.material.color = upgradeDef.displayColor;
        }
        UnityEngine.UI.Image choiceImage = choiceActivator.GetComponent<UnityEngine.UI.Image>();
        if (choiceImage != null)
        {
            choiceImage.color = upgradeDef.displayColor;
        }

        textElement.gameObject.SetActive(true);
        choiceActivator.gameObject.SetActive(true);
    }

    public void ProcessUpgradeSelection(UpgradeDefinition selectedUpgrade, GameObject selectedChoiceObject)
    {
        if (selectedUpgrade == null || picksMade >= picksAllowed || gameManager == null) return;

        Debug.Log($"UIManager processing selection: {selectedUpgrade.formattedDisplayName}");
        gameManager.ApplyUpgrade(selectedUpgrade);
        picksMade++;

        UpgradeChoiceActivator activator = selectedChoiceObject.GetComponent<UpgradeChoiceActivator>();
        if (activator != null)
        {
            activator.SetSelected(true);
            Renderer choiceRenderer = selectedChoiceObject.GetComponent<Renderer>();
            if (choiceRenderer != null) choiceRenderer.material.color = Color.gray;
            UnityEngine.UI.Image choiceImage = selectedChoiceObject.GetComponent<UnityEngine.UI.Image>();
            if (choiceImage != null) choiceImage.color = Color.gray;
        }

        if (picksMade >= picksAllowed)
        {
            instructionText.text = "Upgrades selected!";
            if (wallController) Invoke(nameof(SignalWallControllerToHide), 1.5f);
        }
        else
        {
            instructionText.text = $"Pick {picksAllowed - picksMade} more upgrade(s)";
        }
    }

    public bool CanMakePick()
    {
        return picksMade < picksAllowed;
    }

    void SignalWallControllerToHide()
    {
        if (wallController) wallController.HideWallAndProceed();
    }
}