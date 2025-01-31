using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    private Weapon[] weapons;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Find all weapon scripts in children
        weapons = GetComponentsInChildren<Weapon>(true);
    }

    public void DisableWeapons()
    {
        if (weapons == null || weapons.Length == 0)
        {
            Debug.LogWarning("No weapons found in WeaponHolder!");
            return;
        }

        foreach (Weapon weapon in weapons)
        {
            if (weapon != null)
            {
                weapon.DisableWeapon();
            }
        }
    }

    public void EnableWeapons()
    {
        if (weapons == null || weapons.Length == 0)
        {
            Debug.LogWarning("No weapons found in WeaponHolder!");
            return;
        }

        foreach (Weapon weapon in weapons)
        {
            if (weapon != null)
            {
                weapon.EnableWeapon();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
