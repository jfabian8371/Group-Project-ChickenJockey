using UnityEngine;

public class WallController : MonoBehaviour
{
    // This 'wallObject' should be the top-level GameObject that you want to activate/deactivate
    // to show or hide the entire wall structure including its canvas.
    // In your case, this is likely 'UpgradeWallObject'.
    [Tooltip("The main GameObject representing the physical upgrade wall. This will be activated/deactivated.")]
    public GameObject physicalWallObject;

    // Reference to the UpgradeUIManager script, which is likely on the WorldSpace Canvas
    // or a child panel of that canvas.
    [Tooltip("Reference to the UpgradeUIManager component.")]
    public UpgradeUIManager upgradeUIManager;

    private GameManager gameManager;
    private bool isWallVisible = false;

    void Awake() // Changed from Start to Awake for earlier initialization if needed
    {
        if (!physicalWallObject)
        {
            Debug.LogError("Physical Wall Object not assigned to WallController!", this.gameObject);
        }
        else
        {
            physicalWallObject.SetActive(false); // Ensure it's hidden at start
            isWallVisible = false;
        }

        if (!upgradeUIManager)
        {
            Debug.LogError("UpgradeUIManager not assigned to WallController!", this.gameObject);
        }
    }

    // Called by GameManager to set up necessary references
    public void InitializeController(GameManager gm, PlayerHealth playerHealth)
    {
        gameManager = gm;
        if (upgradeUIManager)
        {
            // Pass necessary references to UpgradeUIManager
            upgradeUIManager.InitializeManagerReferences(gm, this, playerHealth);
        }
        else
        {
            Debug.LogError("Cannot initialize UpgradeUIManager from WallController because it's not assigned.", this.gameObject);
        }
    }

    public void ShowWallAndPrepareUpgrades(int numberOfPicks)
    {
        if (!physicalWallObject || !upgradeUIManager || !gameManager)
        {
            Debug.LogError("WallController cannot show wall. Missing references: " +
                           (!physicalWallObject ? "PhysicalWall " : "") +
                           (!upgradeUIManager ? "UpgradeUIManager " : "") +
                           (!gameManager ? "GameManager" : ""), this.gameObject);
            return;
        }

        if (isWallVisible)
        {
            Debug.LogWarning("ShowWall called, but wall is already visible. Repopulating UI.", this.gameObject);
        }

        // --- Add animation/tweening logic here to make the wall appear ---
        // Example: Move wall into position, fade in, etc.
        // For now, just activate it:
        physicalWallObject.SetActive(true);
        isWallVisible = true;
        // --- End of appearance animation logic ---

        // Tell UpgradeUIManager to populate the UI elements with new upgrades
        upgradeUIManager.PrepareAndShowUpgradeScreen(numberOfPicks);

        // Unlock cursor and make it visible for UI interaction
        //Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;
        // If using StarterAssetsInputs, you might need to disable look input:
        // FindObjectOfType<StarterAssets.StarterAssetsInputs>()?.SetCursorInputForLook(false);
    }

    public void HideWallAndProceed()
    {
        if (!physicalWallObject) return;
        if (!isWallVisible)
        {
            Debug.LogWarning("HideWallAndProceed called, but wall is already hidden.", this.gameObject);
            // Still proceed in case game logic depends on it
            if (gameManager) gameManager.StartNextRound();
            return;
        }


        // --- Add animation/tweening logic here for hiding ---
        // For now, just deactivate it:
        physicalWallObject.SetActive(false);
        isWallVisible = false;
        // --- End of hiding animation logic ---

        // Lock cursor and hide it again for gameplay
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
        // If using StarterAssetsInputs, re-enable look input:
        // FindObjectOfType<StarterAssets.StarterAssetsInputs>()?.SetCursorInputForLook(true);


        if (gameManager)
        {
            gameManager.StartNextRound(); // Tell GameManager to proceed to the next game state
        }
    }

    // Optional: A public getter if other scripts need to know wall's visibility
    public bool IsWallVisible()
    {
        return isWallVisible;
    }
}