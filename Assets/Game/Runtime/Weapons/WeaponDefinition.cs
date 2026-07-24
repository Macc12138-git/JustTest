using JustTest.Game.Combat;
using UnityEngine;

namespace JustTest.Game.Weapons
{
    [CreateAssetMenu(fileName = "WeaponDefinition", menuName = "JustTest/Weapons/Weapon Definition")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Weapon";
        [SerializeField] private WeaponBasicComboDefinition basicCombo;
        [SerializeField] private WeaponSkillDefinition skill;
        [SerializeField] private CombatStatusType qteTriggerStatus;
        [SerializeField] private WeaponQteDefinition qteAction;
        [SerializeField] private Color debugColor = Color.white;

        internal string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;

        internal WeaponBasicComboDefinition BasicCombo => basicCombo;

        internal WeaponSkillDefinition Skill => skill;

        internal CombatStatusType QteTriggerStatus => qteTriggerStatus;

        internal WeaponQteDefinition QteAction => qteAction;

        internal Color DebugColor => debugColor;

        internal bool IsValid =>
            basicCombo != null &&
            basicCombo.IsValid &&
            skill != null &&
            skill.IsValid &&
            qteAction != null &&
            qteAction.IsValid &&
            qteTriggerStatus >= CombatStatusType.Unbalanced &&
            qteTriggerStatus <= CombatStatusType.Stunned;
    }
}
