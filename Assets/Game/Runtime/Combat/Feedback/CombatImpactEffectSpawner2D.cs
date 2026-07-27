using System.Collections.Generic;
using UnityEngine;

namespace JustTest.Game.Combat
{
    public sealed class CombatImpactEffectSpawner2D : MonoBehaviour
    {
        private readonly List<CombatImpactEffect2D> activeEffects = new();

        private void OnDisable()
        {
            ResetEffects();
        }

        internal void Spawn(
            CombatImpactEffect2D prefab,
            Vector3 position,
            int direction,
            float scale,
            float lifetime)
        {
            if (prefab == null ||
                (direction != -1 && direction != 1) ||
                !IsFinitePositive(scale) ||
                !IsFinitePositive(lifetime))
            {
                return;
            }

            RemoveDestroyedEffects();
            Quaternion rotation = Quaternion.Euler(0f, 0f, direction == 1 ? 0f : 180f);
            CombatImpactEffect2D instance = Instantiate(prefab, position, rotation, transform);
            instance.transform.localScale = Vector3.one * scale;
            instance.Play(lifetime);
            activeEffects.Add(instance);
        }

        internal void ResetEffects()
        {
            for (int index = 0; index < activeEffects.Count; index++)
            {
                CombatImpactEffect2D effect = activeEffects[index];
                if (effect != null)
                {
                    Destroy(effect.gameObject);
                }
            }

            activeEffects.Clear();
        }

        private void RemoveDestroyedEffects()
        {
            for (int index = activeEffects.Count - 1; index >= 0; index--)
            {
                if (activeEffects[index] == null)
                {
                    activeEffects.RemoveAt(index);
                }
            }
        }

        private static bool IsFinitePositive(float value)
        {
            return value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
