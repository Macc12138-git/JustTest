using System;
using UnityEngine;

namespace JustTest.Game.Combat
{
    [CreateAssetMenu(fileName = "CombatStatusEventChannel", menuName = "JustTest/Combat/Status Event Channel")]
    public sealed class CombatStatusEventChannel : ScriptableObject
    {
        internal event Action<CombatStatusSignal> StatusApplied;

        internal event Action<CombatStatusSignal> StatusEnded;

        internal void RaiseStatusApplied(
            CombatStatusController target,
            in CombatStatusEvent statusEvent)
        {
            StatusApplied?.Invoke(new CombatStatusSignal(target, statusEvent));
        }

        internal void RaiseStatusEnded(
            CombatStatusController target,
            in CombatStatusEvent statusEvent)
        {
            StatusEnded?.Invoke(new CombatStatusSignal(target, statusEvent));
        }
    }
}
