using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections; // For Coroutine

public class TitleScreen : MonoBehaviour
{
    public AudioClip hoverSound;      // Sound when hovering over button
    public AudioClip clickSound;      // Sound when clicking quit
    public AudioClip startClickSound; // Sound when clicking start
    public AudioSource audioSource;

    public Button startButton; // Reference to the start button
    public Button quitButton;  // Reference to the quit button

    private void Start()
    {
        // Add EventTrigger for hover on the start button
        AddHoverEffect(startButton);
        // Add EventTrigger for hover on the quit button
        AddHoverEffect(quitButton);

        // Add listener for button clicks
        startButton.onClick.AddListener(OnStartButtonClick);
        quitButton.onClick.AddListener(OnQuitButtonClick);
    }

    // Adds hover effect to the given button
    private void AddHoverEffect(Button button)
    {
        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();

        // Create a new entry for pointer enter (hover over the button)
        EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry();
        pointerEnterEntry.eventID = EventTriggerType.PointerEnter;
        pointerEnterEntry.callback.AddListener((data) => { PlayHoverSound(); });
        trigger.triggers.Add(pointerEnterEntry);

        // Create a new entry for pointer exit (hover out from the button)
        EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry();
        pointerExitEntry.eventID = EventTriggerType.PointerExit;
        pointerExitEntry.callback.AddListener((data) => { StopHoverSound(); });
        trigger.triggers.Add(pointerExitEntry);
    }

    // Play hover sound
    private void PlayHoverSound()
    {
        if (hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }

    // Stop hover sound (optional)
    private void StopHoverSound()
    {
        // Optional: Stop any sound
        // audioSource.Stop();
    }

    // Play start click sound and start the game after it finishes
    private void OnStartButtonClick()
    {
        if (startClickSound != null)
        {
            audioSource.PlayOneShot(startClickSound);
            StartCoroutine(LoadGameAfterSound(startClickSound.length));
        }
        else
        {
            SceneManager.LoadScene("Potential New Map"); // fallback if no sound
        }
    }

    // Wait for sound before loading scene
    private IEnumerator LoadGameAfterSound(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("Potential New Map");
    }

    // Play quit click sound and quit
    private void OnQuitButtonClick()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
        Application.Quit();
    }
}
