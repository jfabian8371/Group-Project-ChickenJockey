using TMPro;
using UnityEngine;

public class AmmoDisplay : MonoBehaviour
{
    public WeaponSwitcher weaponSwitcher; // Assign in Inspector
    public TMP_Text ammoText;

    private IWeapon currentWeapon;

    void Update()
    {
        GameObject activeWeapon = GetActiveWeapon();
        if (activeWeapon == null || ammoText == null)
        {
            ammoText.text = "";
            return;
        }

        currentWeapon = activeWeapon.GetComponent<IWeapon>();
        if (currentWeapon == null)
        {
            ammoText.text = "∞"; // For melee weapons
            return;
        }

        ammoText.text = currentWeapon.IsReloading
            ? "Reload"
            : $"{currentWeapon.CurrentAmmo} / {currentWeapon.ClipSize}";
    }

    private GameObject GetActiveWeapon()
    {
        foreach (GameObject weapon in weaponSwitcher.weapons)
        {
            if (weapon != null && weapon.activeSelf)
                return weapon;
        }
        return null;
    }
}
