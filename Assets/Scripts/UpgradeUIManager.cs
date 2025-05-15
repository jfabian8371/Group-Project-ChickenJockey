// UpgradeUIManager.cs
using UnityEngine;
// using UnityEngine.UI; // Button component might not be directly used for clicks anymore
using TMPro;
using System.Collections.Generic;
using System.Linq;

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
    // Optional: If you want to color the background of the text or a panel behind the text
    // public UnityEngine.UI.Image[] choiceBackgroundImages = new UnityEngine.UI.Image[4];


    private List<UpgradeDefinition> allPossibleRandomUpgrades = new List<UpgradeDefinition>();
    private UpgradeDefinition healToFullUpgrade;

    // No longer storing currentDisplayedUpgrades list here, as UpgradeChoiceActivator holds its own.

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
        // Optional: Check for background images length if you use them
        // if (choiceBackgroundImages.Length != 0 && choiceBackgroundImages.Length != 4) { ... }


        InitializeUpgradeDefinitions();
        if (upgradePanelContainer) upgradePanelContainer.SetActive(false);

        // Ensure all shootable choices are initially reset and potentially hidden if part of the panel
        for (int i = 0; i < shootableChoices.Length; i++)
        {
            if (shootableChoices[i] != null)
            {
                shootableChoices[i].ResetChoice(); // Initialize them
                // shootableChoices[i].gameObject.SetActive(false); // If they should only appear when populated
            }
            else
            {
                Debug.LogError($"Shootable Choice at index {i} is not assigned!", this);
            }
        }
    }

    void InitializeUpgradeDefinitions()
    {
        healToFullUpgrade = new UpgradeDefinition("heal_full", UpgradeType.HealToFull, "Heal to Full", 0f);

        allPossibleRandomUpgrades.Add(new UpgradeDefinition("max_health_boost", UpgradeType.IncreaseMaxHealth, "+{0} Max Health", 10f));
        allPossibleRandomUpgrades.Add(new UpgradeDefinition("dmg_reduction", UpgradeType.DamageReduction, "+{0}% Damage Reduction", 5f));
        allPossibleRandomUpgrades.Add(new UpgradeDefinition("move_speed_boost", UpgradeType.MovementSpeed, "+{0}% Movement Speed", 10f));
        allPossibleRandomUpgrades.Add(new UpgradeDefinition("gun_dmg_boost", UpgradeType.Damage, "+{0}% Gun Damage", 10f));
        allPossibleRandomUpgrades.Add(new UpgradeDefinition("gun_fire_rate_boost", UpgradeType.FireRate, "+{0}% Gun Fire Rate", 15f));
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
        if (!instructionText || shootableChoices.Length != 4 || !playerHealth)
        {
            Debug.LogError("Upgrade UI elements not fully assigned or PlayerHealth missing!", this);
            if (wallController) wallController.HideWallAndProceed();
            return;
        }

        picksAllowed = numberOfPicks;
        picksMade = 0;
        instructionText.text = $"Pick {picksAllowed} upgrade(s)";

        // Reset all choices first
        foreach (UpgradeChoiceActivator choiceActivator in shootableChoices)
        {
            if (choiceActivator != null) choiceActivator.ResetChoice();
        }

        // --- Slot 0: Always Heal to Full ---
        healToFullUpgrade.DetermineAndSetInstanceDetails(useRandomRarity: false);
        SetupChoiceVisuals(shootableChoices[0], choiceDisplayTexts[0], healToFullUpgrade);
        // Optional: SetupChoiceVisuals(shootableChoices[0], choiceDisplayTexts[0], choiceBackgroundImages[0], healToFullUpgrade);

        // --- Slots 1, 2, 3: Random Upgrades ---
        List<UpgradeDefinition> tempAvailableUpgrades = new List<UpgradeDefinition>(allPossibleRandomUpgrades);
        if (playerHealth.IsDamageReductionCapped())
        {
            tempAvailableUpgrades.RemoveAll(ud => ud.type == UpgradeType.DamageReduction);
        }

        for (int i = 0; i < tempAvailableUpgrades.Count - 1; i++) // Fisher-Yates shuffle part
        {
            int randomIndex = Random.Range(i, tempAvailableUpgrades.Count);
            (tempAvailableUpgrades[i], tempAvailableUpgrades[randomIndex]) = (tempAvailableUpgrades[randomIndex], tempAvailableUpgrades[i]);
        }

        for (int i = 1; i < 4; i++) // For choices 1, 2, and 3
        {
            if (shootableChoices[i] == null) continue; // Skip if a choice slot is unassigned

            if (tempAvailableUpgrades.Count > (i - 1))
            {
                UpgradeDefinition chosenDefinition = tempAvailableUpgrades[i - 1];
                UpgradeDefinition rolledUpgrade = new UpgradeDefinition(
                    chosenDefinition.upgradeID, chosenDefinition.type,
                    chosenDefinition.baseDisplayName, chosenDefinition.commonValue
                );
                rolledUpgrade.DetermineAndSetInstanceDetails();

                SetupChoiceVisuals(shootableChoices[i], choiceDisplayTexts[i], rolledUpgrade);
                // Optional: SetupChoiceVisuals(shootableChoices[i], choiceDisplayTexts[i], choiceBackgroundImages[i], rolledUpgrade);
                shootableChoices[i].gameObject.SetActive(true);
            }
            else
            {
                shootableChoices[i].gameObject.SetActive(false);
                if (choiceDisplayTexts[i] != null) choiceDisplayTexts[i].text = ""; // Clear text
                // if (choiceBackgroundImages[i] != null) choiceBackgroundImages[i].gameObject.SetActive(false);
            }
        }
        if (upgradePanelContainer) upgradePanelContainer.SetActive(true);
    }

    // Overload for SetupChoiceVisuals if you have background images to color
    // void SetupChoiceVisuals(UpgradeChoiceActivator choiceActivator, TextMeshProUGUI textElement, UnityEngine.UI.Image backgroundImage, UpgradeDefinition upgradeDef)
    // {
    //     if (choiceActivator == null || textElement == null || upgradeDef == null) return;
    //     choiceActivator.InitializeChoice(upgradeDef, this);
    //     textElement.text = upgradeDef.formattedDisplayName;
    //     if (backgroundImage != null)
    //     {
    //         backgroundImage.color = upgradeDef.displayColor;
    //         backgroundImage.gameObject.SetActive(true);
    //     }
    //     choiceActivator.gameObject.SetActive(true);
    // }

    void SetupChoiceVisuals(UpgradeChoiceActivator choiceActivator, TextMeshProUGUI textElement, UpgradeDefinition upgradeDef)
    {
        if (choiceActivator == null || textElement == null || upgradeDef == null)
        {
            Debug.LogError($"Missing element for setting up choice: Activator null? {choiceActivator == null}, Text null? {textElement == null}, Def null? {upgradeDef == null}", this);
            return;
        }
        choiceActivator.InitializeChoice(upgradeDef, this);
        textElement.text = upgradeDef.formattedDisplayName;

        // If the shootable choice itself has a Renderer for color (e.g., a 3D Cube)
        Renderer choiceRenderer = choiceActivator.GetComponent<Renderer>();
        if (choiceRenderer != null)
        {
            choiceRenderer.material.color = upgradeDef.displayColor; // Make sure material supports color tinting
        }
        // OR, if the visual is a UI Image on the same GameObject as the UpgradeChoiceActivator
        UnityEngine.UI.Image choiceImage = choiceActivator.GetComponent<UnityEngine.UI.Image>();
        if (choiceImage != null)
        {
            choiceImage.color = upgradeDef.displayColor;
        }
        // else if (choiceBackgroundImages are used and assigned for separate visual background coloring)
        // {
        //     // Find the corresponding background image for this choiceActivator index if they are separate
        //     // This requires careful array management if choiceBackgroundImages is used.
        // }

        textElement.gameObject.SetActive(true); // Ensure text is visible
        choiceActivator.gameObject.SetActive(true); // Ensure shootable choice is visible
    }

    // Called by UpgradeChoiceActivator when it's shot
    public void ProcessUpgradeSelection(UpgradeDefinition selectedUpgrade, GameObject selectedChoiceObject)
    {
        if (selectedUpgrade == null || picksMade >= picksAllowed) return;

        Debug.Log($"UIManager processing selection: {selectedUpgrade.formattedDisplayName}");
        gameManager.ApplyUpgrade(selectedUpgrade);
        picksMade++;

        // Find the activator on the selected object and mark it as selected
        UpgradeChoiceActivator activator = selectedChoiceObject.GetComponent<UpgradeChoiceActivator>();
        if (activator != null)
        {
            activator.SetSelected(true); // Mark as selected to prevent re-selection
            // Optionally, make it visually obvious it's selected, e.g., change color, disable collider
            // selectedChoiceObject.GetComponent<Collider>().enabled = false; // Example
            Renderer choiceRenderer = selectedChoiceObject.GetComponent<Renderer>();
            if (choiceRenderer != null) choiceRenderer.material.color = Color.gray; // Example: Gray out
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