using System;
using System.Collections.Generic;
using JustTest.Game.Presentation;
using UnityEngine;

namespace JustTest.Game.Combat
{
    public sealed class CombatFeedbackCoordinator : MonoBehaviour
    {
        [Serializable]
        private sealed class TargetBinding
        {
            [SerializeField] private DamageReceiver receiver;
            [SerializeField] private CombatHitFlash2D hitFlash;
            [SerializeField] private Transform impactAnchor;

            [NonSerialized] private CombatFeedbackCoordinator owner;

            internal TargetBinding()
            {
            }

            internal TargetBinding(
                DamageReceiver receiver,
                CombatHitFlash2D hitFlash,
                Transform impactAnchor)
            {
                this.receiver = receiver;
                this.hitFlash = hitFlash;
                this.impactAnchor = impactAnchor;
            }

            internal bool IsValid =>
                receiver != null &&
                hitFlash != null &&
                impactAnchor != null;

            internal CombatHitFlash2D HitFlash => hitFlash;
            internal int ReceiverId => receiver != null ? receiver.GetInstanceID() : 0;

            internal void Initialize(CombatFeedbackCoordinator coordinator)
            {
                owner = coordinator;
            }

            internal void Subscribe()
            {
                receiver.HitProcessed += OnHitProcessed;
                receiver.CombatStateReset += OnCombatStateReset;
            }

            internal void Unsubscribe()
            {
                if (receiver == null)
                {
                    return;
                }

                receiver.HitProcessed -= OnHitProcessed;
                receiver.CombatStateReset -= OnCombatStateReset;
            }

            internal Vector3 GetImpactPosition(int attackDirection, Vector2 offset)
            {
                Vector3 position = impactAnchor.position;
                position.x -= attackDirection * Mathf.Abs(offset.x);
                position.y += offset.y;
                return position;
            }

            private void OnHitProcessed(HitResolution resolution)
            {
                owner?.ProcessHit(this, resolution);
            }

            private void OnCombatStateReset()
            {
                owner?.ResetFeedback(this);
            }
        }

        [Serializable]
        private sealed class SourceBinding
        {
            [SerializeField] private MonoBehaviour[] sources;
            [SerializeField] private CombatAttackRecoil2D recoil;

            private int lastRecoilSourceId;
            private int lastRecoilAttackInstanceId;

            internal SourceBinding()
            {
            }

            internal SourceBinding(MonoBehaviour[] sources, CombatAttackRecoil2D recoil)
            {
                this.sources = sources;
                this.recoil = recoil;
            }

            internal bool IsValid
            {
                get
                {
                    if (recoil == null || sources == null || sources.Length == 0)
                    {
                        return false;
                    }

                    for (int index = 0; index < sources.Length; index++)
                    {
                        if (sources[index] == null)
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            internal bool CanRegister(Dictionary<int, SourceBinding> lookup)
            {
                for (int index = 0; index < sources.Length; index++)
                {
                    int sourceId = sources[index].GetInstanceID();
                    if (lookup.ContainsKey(sourceId))
                    {
                        return false;
                    }
                }

                return true;
            }

            internal void Register(Dictionary<int, SourceBinding> lookup)
            {
                for (int index = 0; index < sources.Length; index++)
                {
                    lookup.Add(sources[index].GetInstanceID(), this);
                }
            }

            internal void Unregister(Dictionary<int, SourceBinding> lookup)
            {
                for (int index = 0; index < sources.Length; index++)
                {
                    int sourceId = sources[index] != null ? sources[index].GetInstanceID() : 0;
                    if (sourceId != 0 &&
                        lookup.TryGetValue(sourceId, out SourceBinding registered) &&
                        ReferenceEquals(registered, this))
                    {
                        lookup.Remove(sourceId);
                    }
                }
            }

            internal void RequestRecoil(
                int sourceId,
                int attackInstanceId,
                int attackDirection,
                CombatFeedbackProfile profile)
            {
                if (sourceId == lastRecoilSourceId &&
                    attackInstanceId == lastRecoilAttackInstanceId)
                {
                    return;
                }

                lastRecoilSourceId = sourceId;
                lastRecoilAttackInstanceId = attackInstanceId;
                recoil.RequestRecoil(
                    attackDirection,
                    profile.RecoilBodyDistance,
                    profile.RecoilWeaponRotation,
                    profile.RecoilDuration,
                    profile.RecoilRecoveryCurve);
            }

            internal void Reset()
            {
                lastRecoilSourceId = 0;
                lastRecoilAttackInstanceId = 0;
                recoil?.ResetRecoil();
            }
        }

        [SerializeField] private CombatFeedbackConfig config;
        [SerializeField] private CombatHitStopController hitStopController;
        [SerializeField] private CombatCameraShake2D cameraShake;
        [SerializeField] private CombatImpactEffectSpawner2D impactSpawner;
        [SerializeField] private TargetBinding[] targets;
        [SerializeField] private SourceBinding[] sources;

        private Dictionary<int, SourceBinding> sourceLookup;
        private Dictionary<int, TargetBinding> runtimeTargets;
        private Dictionary<int, SourceBinding> runtimeSources;
        private HashSet<int> registeredTargetIds;
        private int nextRuntimeRegistrationId = 1;
        private bool ready;

        private void Awake()
        {
            ready =
                config != null &&
                hitStopController != null &&
                cameraShake != null &&
                impactSpawner != null &&
                targets != null &&
                targets.Length > 0 &&
                sources != null &&
                sources.Length > 0;
            if (ready)
            {
                sourceLookup = new Dictionary<int, SourceBinding>();
                runtimeTargets = new Dictionary<int, TargetBinding>();
                runtimeSources = new Dictionary<int, SourceBinding>();
                registeredTargetIds = new HashSet<int>();
                ready = InitializeTargets() && InitializeSources();
            }

            if (ready)
            {
                return;
            }

            Debug.LogError($"{nameof(CombatFeedbackCoordinator)} is missing or duplicating an Inspector reference.", this);
            enabled = false;
        }

        private void OnEnable()
        {
            if (!ready)
            {
                return;
            }

            for (int index = 0; index < targets.Length; index++)
            {
                targets[index].Subscribe();
            }

            foreach (TargetBinding target in runtimeTargets.Values)
            {
                target.Subscribe();
            }
        }

        private void OnDisable()
        {
            if (targets != null)
            {
                for (int index = 0; index < targets.Length; index++)
                {
                    targets[index]?.Unsubscribe();
                    targets[index]?.HitFlash?.ResetFlash();
                }
            }

            if (runtimeTargets != null)
            {
                foreach (TargetBinding target in runtimeTargets.Values)
                {
                    target.Unsubscribe();
                    target.HitFlash.ResetFlash();
                }
            }

            ResetSourceFeedback();
            hitStopController?.ResetStop();
            cameraShake?.ResetShake();
            impactSpawner?.ResetEffects();
        }

        private bool InitializeTargets()
        {
            for (int index = 0; index < targets.Length; index++)
            {
                TargetBinding target = targets[index];
                if (target == null || !target.IsValid)
                {
                    return false;
                }

                if (!registeredTargetIds.Add(target.ReceiverId))
                {
                    return false;
                }

                target.Initialize(this);
            }

            return true;
        }

        private bool InitializeSources()
        {
            for (int index = 0; index < sources.Length; index++)
            {
                SourceBinding source = sources[index];
                if (source == null || !source.IsValid || !source.CanRegister(sourceLookup))
                {
                    return false;
                }

                source.Register(sourceLookup);
            }

            return true;
        }

        internal bool TryRegisterRuntime(
            DamageReceiver receiver,
            CombatHitFlash2D hitFlash,
            Transform impactAnchor,
            MonoBehaviour[] feedbackSources,
            CombatAttackRecoil2D recoil,
            out CombatFeedbackRegistration registration)
        {
            registration = default;
            TargetBinding target = new TargetBinding(receiver, hitFlash, impactAnchor);
            SourceBinding source = new SourceBinding(feedbackSources, recoil);
            if (!ready ||
                !target.IsValid ||
                !source.IsValid ||
                registeredTargetIds.Contains(target.ReceiverId) ||
                !source.CanRegister(sourceLookup))
            {
                return false;
            }

            int registrationId = nextRuntimeRegistrationId;
            nextRuntimeRegistrationId = nextRuntimeRegistrationId == int.MaxValue
                ? 1
                : nextRuntimeRegistrationId + 1;
            while (runtimeTargets.ContainsKey(registrationId))
            {
                registrationId = nextRuntimeRegistrationId;
                nextRuntimeRegistrationId = nextRuntimeRegistrationId == int.MaxValue
                    ? 1
                    : nextRuntimeRegistrationId + 1;
            }

            target.Initialize(this);
            source.Register(sourceLookup);
            runtimeTargets.Add(registrationId, target);
            runtimeSources.Add(registrationId, source);
            registeredTargetIds.Add(target.ReceiverId);
            if (isActiveAndEnabled)
            {
                target.Subscribe();
            }

            registration = new CombatFeedbackRegistration(registrationId);
            return true;
        }

        internal bool UnregisterRuntime(CombatFeedbackRegistration registration)
        {
            if (!registration.IsValid ||
                runtimeTargets == null ||
                !runtimeTargets.TryGetValue(registration.Id, out TargetBinding target) ||
                !runtimeSources.TryGetValue(registration.Id, out SourceBinding source))
            {
                return false;
            }

            target.Unsubscribe();
            target.HitFlash.ResetFlash();
            source.Unregister(sourceLookup);
            source.Reset();
            registeredTargetIds.Remove(target.ReceiverId);
            runtimeTargets.Remove(registration.Id);
            runtimeSources.Remove(registration.Id);
            return true;
        }

        private void ProcessHit(TargetBinding target, HitResolution resolution)
        {
            if (!resolution.Result.WasApplied || resolution.Request.FeedbackTier == CombatFeedbackTier.None)
            {
                return;
            }

            CombatFeedbackProfile profile = config.GetProfile(resolution.Request.FeedbackTier);
            if (profile == null)
            {
                return;
            }

            float multiplier = resolution.Result.KilledTarget ? profile.KillMultiplier : 1f;
            hitStopController.RequestStop(profile.HitStopDuration * multiplier);
            cameraShake.RequestShake(
                profile.CameraShakeDuration * multiplier,
                profile.CameraShakeAmplitude * multiplier,
                profile.CameraShakeFrequency);
            target.HitFlash.RequestFlash(
                profile.FlashColor,
                profile.FlashDuration * multiplier);

            if (sourceLookup.TryGetValue(resolution.Request.SourceId, out SourceBinding source))
            {
                source.RequestRecoil(
                    resolution.Request.SourceId,
                    resolution.Request.AttackInstanceId,
                    resolution.Request.AttackDirection,
                    profile);
            }

            impactSpawner.Spawn(
                profile.ImpactPrefab,
                target.GetImpactPosition(
                    resolution.Request.AttackDirection,
                    profile.ImpactOffset),
                resolution.Request.AttackDirection,
                profile.ImpactScale * multiplier,
                profile.ImpactLifetime * multiplier);
        }

        private void ResetFeedback(TargetBinding target)
        {
            target.HitFlash.ResetFlash();
            ResetSourceFeedback();
            hitStopController.ResetStop();
            cameraShake.ResetShake();
            impactSpawner.ResetEffects();
        }

        private void ResetSourceFeedback()
        {
            if (sources == null)
            {
                return;
            }

            for (int index = 0; index < sources.Length; index++)
            {
                sources[index]?.Reset();
            }

            if (runtimeSources == null)
            {
                return;
            }

            foreach (SourceBinding source in runtimeSources.Values)
            {
                source.Reset();
            }
        }
    }
}
