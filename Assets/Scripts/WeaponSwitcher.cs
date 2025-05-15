using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    [Tooltip("List of weapon GameObjects in order: 1 = Element 0, 2 = Element 1, etc.")]
    public GameObject[] weapons;

    private int currentWeaponIndex = -1; // -1 = no weapon equipped

    void Start()
    {
        UnequipAllWeapons();

        // Optionally equip the first weapon at start:
        // EquipWeapon(0);
    }

    void Update()
    {
        // Keys 1–5 (Alpha1 to Alpha5) map to index 0–4
        for (int i = 0; i < weapons.Length && i < 9; i++) // up to Alpha9
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                EquipWeapon(i);
                return;
            }
        }

        // Optionally, key 0 (Alpha0) maps to index 9
        if (weapons.Length > 9 && Input.GetKeyDown(KeyCode.Alpha0))
        {
            EquipWeapon(9);
        }
    }

    void EquipWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length)
        {
            Debug.LogWarning("Weapon index out of range: " + index);
            return;
        }

        if (currentWeaponIndex == index)
        {
            // Unequip current weapon
            UnequipAllWeapons();
            currentWeaponIndex = -1;
            Debug.Log("Unequipped weapon at index: " + index);
        }
        else
        {
            // Equip the selected weapon and deactivate others
            for (int i = 0; i < weapons.Length; i++)
            {
                weapons[i].SetActive(i == index);
            }
            currentWeaponIndex = index;
            Debug.Log("Equipped weapon: " + weapons[index].name);
        }
    }

    void UnequipAllWeapons()
    {
        foreach (GameObject weapon in weapons)
        {
            if (weapon != null)
                weapon.SetActive(false);
        }
    }
}
