using UnityEngine;

public class TestPlayerHealth : MonoBehaviour
{
    public PlayerHealth playerHealth;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            playerHealth.TakeDamage(10f);
            Debug.Log("Player took 10 damage.");
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            playerHealth.Heal(10f);
            Debug.Log("Player healed 10 health.");
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            playerHealth.HealToFull();
            Debug.Log("Player healed to full.");
        }
    }
}
