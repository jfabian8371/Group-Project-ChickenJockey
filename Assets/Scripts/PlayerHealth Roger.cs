using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthRoger
{
    public float Health, MaxHealth, Width, Height;

    [SerializeField]
    private RectTransform healthBar;

    private void SetMaxHealth(float maxHealth) {
        MaxHealth = maxHealth;
    }

    public void SetHealth(float health) {
        Health = health;
        float newWidth = (Health / MaxHealth) * Width;

        healthBar.sizeDelta = new Vector2(newWidth, Height);
    }
}
