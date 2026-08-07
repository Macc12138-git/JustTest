using System;
using JustTest.Game.Combat;
using UnityEngine;

namespace JustTest.Game.Presentation
{
    [Serializable]
    public sealed class AttackAnimationBinding
    {
        [SerializeField] private AttackDefinition attack;
        [SerializeField] private string stateName = "Attack";
        [SerializeField, Range(0.01f, 0.98f)] private float windupEndNormalized = 0.3f;
        [SerializeField, Range(0.02f, 0.99f)] private float activeEndNormalized = 0.65f;

        internal AttackAnimationBinding()
        {
        }

        internal AttackAnimationBinding(
            AttackDefinition attack,
            string stateName,
            float windupEndNormalized,
            float activeEndNormalized)
        {
            this.attack = attack;
            this.stateName = stateName;
            this.windupEndNormalized = windupEndNormalized;
            this.activeEndNormalized = activeEndNormalized;
        }

        internal AttackDefinition Attack => attack;
        internal string StateName => stateName;
        internal bool IsValid =>
            attack != null &&
            !string.IsNullOrWhiteSpace(stateName) &&
            windupEndNormalized > 0f &&
            activeEndNormalized > windupEndNormalized &&
            activeEndNormalized < 1f;

        internal float EvaluateNormalizedTime(AttackPhase phase, float phaseProgress)
        {
            float progress = Mathf.Clamp01(phaseProgress);
            switch (phase)
            {
                case AttackPhase.Windup:
                    return Mathf.Lerp(0f, windupEndNormalized, progress);
                case AttackPhase.Active:
                    return Mathf.Lerp(windupEndNormalized, activeEndNormalized, progress);
                case AttackPhase.Recovery:
                    return Mathf.Lerp(activeEndNormalized, 1f, progress);
                default:
                    return 0f;
            }
        }

    }
}
