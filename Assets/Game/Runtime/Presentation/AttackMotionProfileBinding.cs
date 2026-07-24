using System;
using JustTest.Game.Combat;
using UnityEngine;

namespace JustTest.Game.Presentation
{
    [Serializable]
    public sealed class AttackMotionProfileBinding
    {
        [SerializeField] private AttackDefinition attack;
        [SerializeField] private CombatMotionProfile motionProfile;

        internal AttackDefinition Attack => attack;
        internal CombatMotionProfile MotionProfile => motionProfile;
    }
}
