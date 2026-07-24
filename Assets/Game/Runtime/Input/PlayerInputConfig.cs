using UnityEngine;

namespace JustTest.Game.Input
{
    [CreateAssetMenu(fileName = "PlayerInputConfig", menuName = "JustTest/Player/Input Config")]
    public sealed class PlayerInputConfig : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField] private KeyCode moveLeftKey = KeyCode.A;
        [SerializeField] private KeyCode moveRightKey = KeyCode.D;
        [SerializeField] private KeyCode moveDownKey = KeyCode.S;

        [Header("Actions")]
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode primaryAttackKey = KeyCode.J;
        [SerializeField] private KeyCode rollKey = KeyCode.K;
        [SerializeField] private KeyCode weaponSkillKey = KeyCode.L;
        [SerializeField] private KeyCode resetKey = KeyCode.R;

        [Header("Weapons")]
        [SerializeField] private KeyCode weaponSlotOneKey = KeyCode.Alpha1;
        [SerializeField] private KeyCode weaponSlotTwoKey = KeyCode.Alpha2;
        [SerializeField] private KeyCode weaponSlotThreeKey = KeyCode.Alpha3;

        internal KeyCode MoveLeftKey => moveLeftKey;
        internal KeyCode MoveRightKey => moveRightKey;
        internal KeyCode MoveDownKey => moveDownKey;
        internal KeyCode JumpKey => jumpKey;
        internal KeyCode PrimaryAttackKey => primaryAttackKey;
        internal KeyCode RollKey => rollKey;
        internal KeyCode WeaponSkillKey => weaponSkillKey;
        internal KeyCode ResetKey => resetKey;
        internal KeyCode WeaponSlotOneKey => weaponSlotOneKey;
        internal KeyCode WeaponSlotTwoKey => weaponSlotTwoKey;
        internal KeyCode WeaponSlotThreeKey => weaponSlotThreeKey;
    }
}
