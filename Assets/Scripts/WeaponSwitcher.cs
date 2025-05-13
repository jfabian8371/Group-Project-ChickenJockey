using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    public GameObject[] weapons;
    private int currentWeaponIndex = -1; // -1 = no weapon equipped

    void Start()
    {
        UnequipAllWeapons();
    }

    void Update()
    {
        // Keys 1–9 (Alpha1 to Alpha9) map to index 0–8
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                ToggleWeapon(i);
                return;
            }
        }

        // Key 0 (Alpha0) maps to index 9 (weapon 10)
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            ToggleWeapon(9);
        }
    }

    void ToggleWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length)
            return;

        if (currentWeaponIndex == index)
        {
            UnequipAllWeapons();
            currentWeaponIndex = -1;
        }
        else
        {
            SelectWeapon(index);
            currentWeaponIndex = index;
        }
    }

    void SelectWeapon(int index)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].SetActive(i == index);
        }
    }

    void UnequipAllWeapons()
    {
        foreach (GameObject weapon in weapons)
        {
            weapon.SetActive(false);
        }
    }
}
