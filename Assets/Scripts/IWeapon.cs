public interface IWeapon
{
    bool IsReloading { get; }
    int CurrentAmmo { get; }
    int ClipSize { get; }
}
