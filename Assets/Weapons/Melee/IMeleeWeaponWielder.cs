using UnityEngine;

public interface IMeleeWeaponWielder
{
    public Transform Transform { get; }
    public BetterInputManager InputManager { get; }
}
