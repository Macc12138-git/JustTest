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
        [SerializeField] private KeyCode rollKey = KeyCode.K;
        [SerializeField] private KeyCode resetKey = KeyCode.R;

        internal KeyCode MoveLeftKey => moveLeftKey;
        internal KeyCode MoveRightKey => moveRightKey;
        internal KeyCode MoveDownKey => moveDownKey;
        internal KeyCode JumpKey => jumpKey;
        internal KeyCode RollKey => rollKey;
        internal KeyCode ResetKey => resetKey;
    }
}
