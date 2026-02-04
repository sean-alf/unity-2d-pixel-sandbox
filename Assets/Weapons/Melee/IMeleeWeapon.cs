using System;
using UnityEngine.Events;

public interface IMeleeWeapon
{
    public void Attack(IMeleeWeaponWielder wielder);
    public void RegisterOnAttackFinished(UnityAction onFinished);
    public void UnregisterOnAttackFinished(UnityAction onFinished);
}
