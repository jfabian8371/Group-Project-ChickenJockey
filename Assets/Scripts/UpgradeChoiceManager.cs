// UpgradeChoiceActivator.cs
using UnityEngine;
using UnityEngine.UI; // For Image component if you're coloring a UI Image
using TMPro; // For TextMeshPro if you're setting text directly here

public class UpgradeChoiceActivator : MonoBehaviour
{
    private UpgradeDefinition _assignedUpgrade;
    private UpgradeUIManager _uiManager;
    private bool _isSelected = false; // To prevent multiple selections of the same choice

    // Optional: References to visual components on this choice object if not handled by UIManager directly
    // public Image backgroundImage;
    // public TextMeshProUGUIdisplayText;

    // Call this from UpgradeUIManager to assign an upgrade and the manager
    public void InitializeChoice(UpgradeDefinition upgradeDef, UpgradeUIManager manager)
    {
        _assignedUpgrade = upgradeDef;
        _uiManager = manager;
        _isSelected = false; // Reset selection state

        // Update visuals on this specific choice object (text, color)
        // This duplicates some of what UIManager does but makes this component more self-contained for visuals
        // You can choose to have UIManager control all visuals or let this script do it.
        // For simplicity, let's assume UIManager still handles the primary text/color on separate UI elements
        // that are visually associated with this shootable object.
        // If this GameObject *is* the button with Image and Text, then UIManager's SetupButton would have done it.
        // If this is just a collider with a separate visual, UIManager needs to update those visuals.

        // For now, this script primarily handles being "shot".
        // The UIManager will handle setting text/color on the associated visual elements.
        gameObject.SetActive(true); // Ensure this shootable object is active
    }

    // This method is called by the Gun script when this object is shot
    public void ActivateByShooting()
    {
        if (_isSelected || _assignedUpgrade == null || _uiManager == null)
        {
            if (_isSelected) Debug.Log($"Choice '{_assignedUpgrade?.formattedDisplayName}' already selected.");
            return; // Already selected, or not properly initialized
        }

        if (!_uiManager.CanMakePick())
        {
            Debug.Log("Cannot make more picks at this time.");
            // Optionally provide feedback to the player, e.g., a sound
            return;
        }

        Debug.Log($"Upgrade choice '{_assignedUpgrade.formattedDisplayName}' activated by shooting!");
        _isSelected = true;

        // Notify the UpgradeUIManager that this choice was made
        _uiManager.ProcessUpgradeSelection(_assignedUpgrade, this.gameObject);

        // Optional: Visual feedback for selection on this specific object
        // For example, change its color, play a sound, disable its collider
        // GetComponent<Renderer>()?.material.color = Color.green; // Example for a 3D object
        // GetComponent<Collider>()?.enabled = false; // Prevent re-shooting
    }

    public UpgradeDefinition GetAssignedUpgrade()
    {
        return _assignedUpgrade;
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        // Optionally, also disable the collider if it's truly "used up"
        // GetComponent<Collider>()?.enabled = !selected;
    }

    public bool IsSelected()
    {
        return _isSelected;
    }

    // Called by UpgradeUIManager to reset for a new round of choices
    public void ResetChoice()
    {
        _assignedUpgrade = null;
        _isSelected = false;
        // GetComponent<Renderer>()?.material.color = Color.white; // Reset visual if changed
        // GetComponent<Collider>()?.enabled = true;
        gameObject.SetActive(true); // Ensure it's active for the next round
    }
}