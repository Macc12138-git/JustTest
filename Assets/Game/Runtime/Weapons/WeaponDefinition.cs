using JustTest.Game.Combat;
using UnityEngine;

namespace JustTest.Game.Weapons
{
    [CreateAssetMenu(fileName = "WeaponDefinition", menuName = "JustTest/Weapons/Weapon Definition")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Weapon";
        [SerializeField] private AttackDefinition basicAttack;
        [SerializeField] private CombatStatusType qteTriggerStatus;
        [SerializeField] private Color debugColor = Color.white;

        internal string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;

        internal AttackDefinition BasicAttack => basicAttack;

        internal CombatStatusType QteTriggerStatus => qteTriggerStatus;

        internal Color DebugColor => debugColor;

        internal bool IsValid =>
            basicAttack != null &&
            qteTriggerStatus >= CombatStatusType.Unbalanced &&
            qteTriggerStatus <= CombatStatusType.Stunned;
    }
}
