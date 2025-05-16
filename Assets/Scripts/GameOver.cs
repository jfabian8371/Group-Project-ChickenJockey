using UnityEngine;
using UnityEngine.SceneManagement;  // Need this for scene reload
using TMPro;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    [Header("UI References")]
    public GameObject deathText;
    public Image deathOverlay;

    [Header("Fade Settings")]
    public float fadeDuration = 1f;

    private void Awake()
    {
        deathText.SetActive(false);
        SetOverlayAlpha(0f);
    }

    public void ShowDeathScreen()
    {
        deathText.SetActive(true);
        StartCoroutine(FadeOverlayIn());
    }

    private void SetOverlayAlpha(float alpha)
    {
        Color c = deathOverlay.color;
        c.a = alpha;
        deathOverlay.color = c;
    }

    private System.Collections.IEnumerator FadeOverlayIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 0.6f, elapsed / fadeDuration);
            SetOverlayAlpha(alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        SetOverlayAlpha(0.6f);

        // Wait a moment so player can see the full fade (optional)
        yield return new WaitForSeconds(1f);

        // Reload the current active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
