using UnityEngine;
using UnityEngine.UI;

public class PlayerShield
{
    public float Shield, MaxShield, Width, Height;

    [SerializeField]
    private RectTransform shieldBar;

    private void SetMaxShield(float maxShield)
    {
        MaxShield = maxShield;
    }

    public void SetShield(float shield)
    {
        Shield = shield;
        float newWidth = (Shield / MaxShield) * Width;
        shieldBar.sizeDelta = new Vector2(newWidth, Height);
    }
}
