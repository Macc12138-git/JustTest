using UnityEngine;

namespace JustTest.Game.Presentation
{
    public sealed class WeaponVisual2D : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private WeaponPresentationDefinition currentDefinition;
        private bool ready;

        internal SpriteRenderer SpriteRenderer => spriteRenderer;

        private void Awake()
        {
            ready = visualRoot != null && spriteRenderer != null;
            if (ready)
            {
                return;
            }

            Debug.LogError($"{nameof(WeaponVisual2D)} is missing an Inspector reference.", this);
            enabled = false;
        }

        internal void Apply(WeaponPresentationDefinition definition)
        {
            if (!ready || currentDefinition == definition)
            {
                return;
            }

            currentDefinition = definition;
            if (definition == null || !definition.IsValid)
            {
                spriteRenderer.enabled = false;
                return;
            }

            spriteRenderer.sprite = definition.Sprite;
            spriteRenderer.color = definition.Color;
            spriteRenderer.enabled = true;
            visualRoot.localPosition = new Vector3(
                definition.LocalPosition.x,
                definition.LocalPosition.y,
                visualRoot.localPosition.z);
            visualRoot.localRotation = Quaternion.Euler(0f, 0f, definition.LocalRotation);
            visualRoot.localScale = new Vector3(
                definition.LocalScale.x,
                definition.LocalScale.y,
                1f);
        }
    }
}
