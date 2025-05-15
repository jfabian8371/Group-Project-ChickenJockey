using TMPro;
using UnityEngine;

public class AmmoDisplay : MonoBehaviour
{
    public Gun gun; // Drag your Gun object here in the Inspector
    public TMP_Text ammoText;

    private void Update()
    {
        if (gun == null || ammoText == null)
            return;

        if (gun.IsReloading)
        {
            ammoText.text = "Reload";
        }
        else
        {
            ammoText.text = $"{gun.CurrentAmmo} / {gun.clipSize}";
        }
    }
}
