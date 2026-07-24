using System;
using JustTest.Game.Weapons;
using UnityEngine;

namespace JustTest.Game.Presentation
{
    [Serializable]
    public sealed class WeaponMotionProfileBinding
    {
        [SerializeField] private WeaponDefinition weapon;
        [SerializeField] private CombatMotionProfile[] basicComboProfiles =
            Array.Empty<CombatMotionProfile>();

        internal WeaponDefinition Weapon => weapon;

        internal CombatMotionProfile GetBasicComboProfile(int stepIndex)
        {
            if (basicComboProfiles == null || basicComboProfiles.Length == 0)
            {
                return null;
            }

            return stepIndex >= 0 && stepIndex < basicComboProfiles.Length
                ? basicComboProfiles[stepIndex]
                : basicComboProfiles[0];
        }
    }
}
