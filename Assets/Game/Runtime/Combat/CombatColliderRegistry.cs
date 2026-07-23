using System.Collections.Generic;
using UnityEngine;

namespace JustTest.Game.Combat
{
    internal sealed class CombatColliderRegistry
    {
        private readonly Dictionary<int, ICombatHurtbox> hurtboxByColliderId = new();

        private CombatColliderRegistry()
        {
        }

        internal static CombatColliderRegistry Instance { get; } = new();

        internal int Count => hurtboxByColliderId.Count;

        internal bool Register(int colliderId, ICombatHurtbox hurtbox)
        {
            if (colliderId == 0 || hurtbox == null)
            {
                return false;
            }

            if (hurtboxByColliderId.TryGetValue(colliderId, out ICombatHurtbox registeredHurtbox))
            {
                return ReferenceEquals(registeredHurtbox, hurtbox);
            }

            hurtboxByColliderId.Add(colliderId, hurtbox);
            return true;
        }

        internal void Unregister(int colliderId, ICombatHurtbox hurtbox)
        {
            if (!hurtboxByColliderId.TryGetValue(colliderId, out ICombatHurtbox registeredHurtbox) ||
                !ReferenceEquals(registeredHurtbox, hurtbox))
            {
                return;
            }

            hurtboxByColliderId.Remove(colliderId);
        }

        internal bool TryResolve(int colliderId, out ICombatHurtbox hurtbox)
        {
            if (!hurtboxByColliderId.TryGetValue(colliderId, out hurtbox))
            {
                return false;
            }

            if (hurtbox.IsAvailable)
            {
                return true;
            }

            hurtboxByColliderId.Remove(colliderId);
            hurtbox = null;
            return false;
        }

        internal void Clear()
        {
            hurtboxByColliderId.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRuntimeState()
        {
            Instance.Clear();
        }
    }
}
