using UnityEngine;

public enum UpgradeRarity
{
    Common,
    Rare,
    Epic,
    Fixed // For "Heal to Full" which has no rarity scaling
}

public enum UpgradeType
{
    HealToFull,
    IncreaseMaxHealth,
    DamageReduction,
    MovementSpeed,
    Damage,
    FireRate
}

[System.Serializable] // Makes it show up in Inspector if part of a MonoBehaviour list
public class UpgradeDefinition
{
    public string upgradeID; // Unique identifier, e.g., "max_health_boost"
    public UpgradeType type;
    public string baseDisplayName; // e.g., "{0} Max Health", "{0}% Movement Speed"
                                   // Use {0} as a placeholder for the value.
                                   // For "Heal to Full", it can be just "Heal to Full".

    public float commonValue; // Base value for common rarity or fixed value

    // Rarity Multipliers
    private const float RARE_MULTIPLIER = 2f;
    private const float EPIC_MULTIPLIER = 3f;

    // Rarity Probabilities (ensure they sum to 1 or handle normalization)
    public static readonly float COMMON_CHANCE = 0.60f;
    public static readonly float RARE_CHANCE = 0.30f;
    public static readonly float EPIC_CHANCE = 0.10f;

    // --- Instance data for a chosen upgrade ---
    // These are set when an upgrade is "rolled" for the player
    [HideInInspector] public UpgradeRarity chosenRarity;
    [HideInInspector] public float actualValue;
    [HideInInspector] public string formattedDisplayName;
    [HideInInspector] public Color displayColor;

    // Constructor for creating definitions
    public UpgradeDefinition(string id, UpgradeType type, string baseName, float cValue)
    {
        this.upgradeID = id;
        this.type = type;
        this.baseDisplayName = baseName;
        this.commonValue = cValue;
    }

    // Method to prepare this definition instance with a chosen rarity
    // This is called when we "roll" an upgrade to present to the player
    public void DetermineAndSetInstanceDetails(UpgradeRarity forceRarity = UpgradeRarity.Common, bool useRandomRarity = true)
    {
        if (type == UpgradeType.HealToFull)
        {
            chosenRarity = UpgradeRarity.Fixed;
            actualValue = 1; // Value doesn't really matter for heal to full, but set to 1 for consistency
            formattedDisplayName = baseDisplayName; // "Heal to Full"
            displayColor = new Color(0.6f, 1.0f, 0.6f, 1.0f); // Light Green
            return;
        }

        if (useRandomRarity)
        {
            float roll = Random.value;
            if (roll < EPIC_CHANCE) // 0.0 to 0.099...
            {
                chosenRarity = UpgradeRarity.Epic;
            }
            else if (roll < EPIC_CHANCE + RARE_CHANCE) // 0.1 to 0.399...
            {
                chosenRarity = UpgradeRarity.Rare;
            }
            else // 0.4 to 0.999...
            {
                chosenRarity = UpgradeRarity.Common;
            }
        }
        else
        {
            chosenRarity = forceRarity;
        }


        switch (chosenRarity)
        {
            case UpgradeRarity.Common:
                actualValue = commonValue;
                displayColor = Color.white; // Default button color
                break;
            case UpgradeRarity.Rare:
                actualValue = commonValue * RARE_MULTIPLIER;
                displayColor = new Color(0.6f, 0.8f, 1f); // Light Blue
                break;
            case UpgradeRarity.Epic:
                actualValue = commonValue * EPIC_MULTIPLIER;
                displayColor = new Color(0.8f, 0.6f, 1f); // Light Purple
                break;
            default: // Should not happen for non-HealToFull
                actualValue = commonValue;
                displayColor = Color.white;
                break;
        }

        // Format the display name
        // Check if it's a percentage or a flat value based on convention (e.g., presence of '%')
        if (baseDisplayName.Contains("%"))
        {
            // For percentages, we might want to display them as whole numbers if they are like 50 for 50%
            formattedDisplayName = string.Format(baseDisplayName, actualValue.ToString("F0")); // F0 for no decimal places
        }
        else
        {
            // For flat values, decide on decimal places. F0 for whole numbers, F1 for one decimal, etc.
            formattedDisplayName = string.Format(baseDisplayName, actualValue.ToString("F0"));
        }
    }
}