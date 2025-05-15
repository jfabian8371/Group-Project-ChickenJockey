using UnityEngine;

public class TestPlayerHealth : MonoBehaviour
{
    public PlayerHealthManager healthManager;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            healthManager.TakeDamage(10f);
            Debug.Log("Player took 10 damage.");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            healthManager.Heal(10f);
            Debug.Log("Player healed 10 health.");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            healthManager.RechargeShield(10f);
            Debug.Log("Player recharged 10 shield.");
        }
    }
}
