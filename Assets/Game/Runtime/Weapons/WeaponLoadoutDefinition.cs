using System;
using System.Collections.Generic;
using UnityEngine;

namespace JustTest.Game.Weapons
{
    [CreateAssetMenu(fileName = "WeaponLoadoutDefinition", menuName = "JustTest/Weapons/Loadout Definition")]
    public sealed class WeaponLoadoutDefinition : ScriptableObject
    {
        [SerializeField] private WeaponDefinition defaultWeapon;
        [SerializeField] private WeaponDefinition[] startingWeapons = Array.Empty<WeaponDefinition>();

        internal WeaponDefinition DefaultWeapon => defaultWeapon;

        internal IReadOnlyList<WeaponDefinition> StartingWeapons => startingWeapons;

        internal bool IsValid
        {
            get
            {
                if (defaultWeapon == null || !defaultWeapon.IsValid)
                {
                    return false;
                }

                int count = Mathf.Min(startingWeapons?.Length ?? 0, WeaponLoadoutState.Capacity);
                for (int index = 0; index < count; index++)
                {
                    WeaponDefinition weapon = startingWeapons[index];
                    if (weapon != null && !weapon.IsValid)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private void OnValidate()
        {
            if (startingWeapons == null)
            {
                startingWeapons = Array.Empty<WeaponDefinition>();
                return;
            }

            if (startingWeapons.Length > WeaponLoadoutState.Capacity)
            {
                Array.Resize(ref startingWeapons, WeaponLoadoutState.Capacity);
            }
        }
    }
}
