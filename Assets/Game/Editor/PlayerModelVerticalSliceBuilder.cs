using System;
using System.Collections.Generic;
using JustTest.Game.Combat;
using JustTest.Game.Player;
using JustTest.Game.Presentation;
using JustTest.Game.Weapons;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JustTest.Game.Editor
{
    public sealed class PlayerModelVerticalSliceBuilder
    {
        private const string ScenePath = "Assets/Game/Scenes/CombatSandbox.unity";
        private const string ModelPrefabPath =
            "Assets/Game/Prefabs/Player/PlayerModelVerticalSlice.prefab";
        private const string ControllerPath =
            "Assets/Game/Art/Characters/PlayerVerticalSlice/PlayerVerticalSlice.controller";
        private const string AppearancePath =
            "Assets/Game/Data/Presentation/PlayerCharacterAppearance.asset";
        private const string ClipDirectory =
            "Assets/Game/Art/Characters/PlayerVerticalSlice/Animations";
        private const string WeaponPresentationDirectory =
            "Assets/Game/Data/Presentation/Weapons";
        private const string PrototypeSpritePath =
            "Assets/Game/Art/Prototype/PrototypeSquare.asset";

        private const string SkeletonPath = "FacingRoot/SkeletonRoot";
        private const string HipsPath = SkeletonPath + "/Hips";
        private const string TorsoPath = HipsPath + "/Torso";
        private const string HeadPath = TorsoPath + "/Neck/Head";
        private const string HairFrontPath = HeadPath + "/HairFront";
        private const string HairBackPath = HeadPath + "/HairBack";
        private const string CapePath = TorsoPath + "/Cape";
        private const string ArmFrontUpperPath = TorsoPath + "/ArmFrontUpper";
        private const string ArmFrontLowerPath = ArmFrontUpperPath + "/ArmFrontLower";
        private const string ArmBackUpperPath = TorsoPath + "/ArmBackUpper";
        private const string LegFrontUpperPath = HipsPath + "/LegFrontUpper";
        private const string LegBackUpperPath = HipsPath + "/LegBackUpper";

        [MenuItem("Tools/JustTest/Rebuild Player Model Vertical Slice")]
        public static void Build()
        {
            if (Application.isPlaying)
            {
                Debug.LogError("Player model vertical slice cannot be rebuilt in Play Mode.");
                return;
            }

            if (Application.productName != "JustTest")
            {
                Debug.LogError("Player model vertical slice builder is only valid for JustTest.");
                return;
            }

            EnsureFolders();
            Sprite prototypeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PrototypeSpritePath);
            if (prototypeSprite == null)
            {
                Debug.LogError($"Missing prototype sprite: {PrototypeSpritePath}");
                return;
            }

            Dictionary<string, AnimationClip> clips = BuildAnimationClips();
            AnimatorController controller = BuildAnimatorController(clips);
            GameObject modelPrefab = BuildModelPrefab(prototypeSprite, controller);
            CharacterAppearanceDefinition appearance = BuildPresentationAssets(prototypeSprite);
            WireScene(modelPrefab, appearance);

            AssetDatabase.SaveAssets();
            Debug.Log(
                "Player model vertical slice rebuilt. Gameplay colliders and combat timelines were not changed.");
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/Game/Art", "Characters");
            EnsureFolder("Assets/Game/Art/Characters", "PlayerVerticalSlice");
            EnsureFolder("Assets/Game/Art/Characters/PlayerVerticalSlice", "Animations");
            EnsureFolder("Assets/Game/Data/Presentation", "Weapons");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static Dictionary<string, AnimationClip> BuildAnimationClips()
        {
            var clips = new Dictionary<string, AnimationClip>();
            clips.Add("Idle", BuildIdleClip());
            clips.Add("Run", BuildRunClip());
            clips.Add("Jump", BuildJumpClip());
            clips.Add("Fall", BuildFallClip());
            clips.Add("Land", BuildLandClip());
            clips.Add("Roll", BuildRollClip());
            clips.Add("Hurt", BuildHurtClip());
            clips.Add("Controlled", BuildControlledClip());
            clips.Add("Dead", BuildDeadClip());
            clips.Add("QteApproach", BuildQteApproachClip());
            clips.Add("AttackFallback", BuildSwordAttack1Clip("AttackFallback"));
            clips.Add("SwordAttack1", BuildSwordAttack1Clip("SwordAttack1"));
            clips.Add("SwordAttack2", BuildSwordAttack2Clip());
            clips.Add("SwordAttack3", BuildSwordAttack3Clip());
            clips.Add("SwordSkill", BuildSwordSkillClip());
            clips.Add("SwordQte", BuildSwordQteClip());
            return clips;
        }

        private static AnimatorController BuildAnimatorController(
            IReadOnlyDictionary<string, AnimationClip> clips)
        {
            AnimatorController controller =
                AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            }

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            AnimatorState idleState = null;
            foreach (KeyValuePair<string, AnimationClip> pair in clips)
            {
                AnimatorState state = FindState(stateMachine, pair.Key);
                if (state == null)
                {
                    state = stateMachine.AddState(pair.Key);
                }

                state.motion = pair.Value;
                state.writeDefaultValues = true;
                if (pair.Key == "Idle")
                {
                    idleState = state;
                }
            }

            if (idleState != null)
            {
                stateMachine.defaultState = idleState;
            }

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static AnimatorState FindState(AnimatorStateMachine stateMachine, string name)
        {
            ChildAnimatorState[] states = stateMachine.states;
            for (int index = 0; index < states.Length; index++)
            {
                if (states[index].state != null && states[index].state.name == name)
                {
                    return states[index].state;
                }
            }

            return null;
        }

        private static GameObject BuildModelPrefab(
            Sprite sprite,
            RuntimeAnimatorController controller)
        {
            var root = new GameObject("PlayerModelVerticalSlice");
            try
            {
                Animator animator = root.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;

                Transform facingRoot = CreateNode(root.transform, "FacingRoot", Vector2.zero);
                Transform skeletonRoot = CreateNode(facingRoot, "SkeletonRoot", Vector2.zero);
                Transform hips = CreateNode(skeletonRoot, "Hips", new Vector2(0f, -0.05f));
                Transform torso = CreateNode(hips, "Torso", new Vector2(0f, 0.28f));
                Transform neck = CreateNode(torso, "Neck", new Vector2(0f, 0.38f));
                Transform head = CreateNode(neck, "Head", new Vector2(0f, 0.28f));
                Transform hairBack = CreateNode(head, "HairBack", Vector2.zero);
                Transform hairFront = CreateNode(head, "HairFront", Vector2.zero);
                Transform cape = CreateNode(torso, "Cape", new Vector2(-0.05f, 0.12f));

                Transform armBackUpper = CreateNode(
                    torso,
                    "ArmBackUpper",
                    new Vector2(-0.28f, 0.24f));
                Transform armBackLower = CreateNode(
                    armBackUpper,
                    "ArmBackLower",
                    new Vector2(-0.03f, -0.34f));
                CreateNode(armBackLower, "HandBack", new Vector2(0f, -0.28f));

                Transform armFrontUpper = CreateNode(
                    torso,
                    "ArmFrontUpper",
                    new Vector2(0.28f, 0.24f));
                Transform armFrontLower = CreateNode(
                    armFrontUpper,
                    "ArmFrontLower",
                    new Vector2(0.04f, -0.34f));
                Transform handFront = CreateNode(
                    armFrontLower,
                    "HandFront",
                    new Vector2(0f, -0.28f));
                Transform mainHandSocket = CreateNode(
                    handFront,
                    "MainHandSocket",
                    new Vector2(0.02f, -0.03f));
                Transform weaponFeedbackPivot = CreateNode(
                    mainHandSocket,
                    "WeaponFeedbackPivot",
                    Vector2.zero);

                Transform legBackUpper = CreateNode(
                    hips,
                    "LegBackUpper",
                    new Vector2(-0.14f, -0.28f));
                Transform legBackLower = CreateNode(
                    legBackUpper,
                    "LegBackLower",
                    new Vector2(0f, -0.38f));
                CreateNode(legBackLower, "FootBack", new Vector2(0.08f, -0.32f));
                Transform legFrontUpper = CreateNode(
                    hips,
                    "LegFrontUpper",
                    new Vector2(0.14f, -0.28f));
                Transform legFrontLower = CreateNode(
                    legFrontUpper,
                    "LegFrontLower",
                    new Vector2(0f, -0.38f));
                CreateNode(legFrontLower, "FootFront", new Vector2(0.08f, -0.32f));

                BuildBodyParts(
                    sprite,
                    hips,
                    torso,
                    head,
                    hairBack,
                    hairFront,
                    cape,
                    armBackUpper,
                    armBackLower,
                    armFrontUpper,
                    armFrontLower,
                    legBackUpper,
                    legBackLower,
                    legFrontUpper,
                    legFrontLower);
                BuildWeapon(sprite, weaponFeedbackPivot);

                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, ModelPrefabPath);
                return prefab;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void BuildBodyParts(
            Sprite sprite,
            Transform hips,
            Transform torso,
            Transform head,
            Transform hairBack,
            Transform hairFront,
            Transform cape,
            Transform armBackUpper,
            Transform armBackLower,
            Transform armFrontUpper,
            Transform armFrontLower,
            Transform legBackUpper,
            Transform legBackLower,
            Transform legFrontUpper,
            Transform legFrontLower)
        {
            Color skin = new Color(1f, 0.78f, 0.72f);
            Color hair = new Color(0.16f, 0.12f, 0.24f);
            Color coat = new Color(0.18f, 0.62f, 0.78f);
            Color coatDark = new Color(0.09f, 0.28f, 0.4f);
            Color accent = new Color(0.92f, 0.23f, 0.38f);

            CreatePart(sprite, cape, "CapeVisual", new Vector2(-0.08f, -0.12f),
                new Vector2(0.55f, 0.82f), coatDark, 5, 8f);
            CreatePart(sprite, hairBack, "HairBackVisual", new Vector2(-0.03f, -0.1f),
                new Vector2(0.66f, 0.8f), hair, 8);
            CreatePart(sprite, legBackUpper, "LegBackUpperVisual", new Vector2(0f, -0.18f),
                new Vector2(0.18f, 0.42f), coatDark, 10);
            CreatePart(sprite, legBackLower, "LegBackLowerVisual", new Vector2(0f, -0.17f),
                new Vector2(0.15f, 0.4f), skin, 11);
            CreatePart(sprite, legFrontUpper, "LegFrontUpperVisual", new Vector2(0f, -0.18f),
                new Vector2(0.2f, 0.42f), coat, 20);
            CreatePart(sprite, legFrontLower, "LegFrontLowerVisual", new Vector2(0f, -0.17f),
                new Vector2(0.16f, 0.4f), skin, 21);
            CreatePart(sprite, hips, "SkirtVisual", new Vector2(0f, -0.06f),
                new Vector2(0.72f, 0.38f), accent, 25);
            CreatePart(sprite, torso, "TorsoVisual", new Vector2(0f, 0f),
                new Vector2(0.56f, 0.68f), coat, 28);
            CreatePart(sprite, torso, "ChestAccent", new Vector2(0.18f, 0.08f),
                new Vector2(0.12f, 0.34f), accent, 30, -12f);
            CreatePart(sprite, armBackUpper, "ArmBackUpperVisual", new Vector2(0f, -0.16f),
                new Vector2(0.17f, 0.4f), coatDark, 12);
            CreatePart(sprite, armBackLower, "ArmBackLowerVisual", new Vector2(0f, -0.14f),
                new Vector2(0.14f, 0.34f), skin, 13);
            CreatePart(sprite, armFrontUpper, "ArmFrontUpperVisual", new Vector2(0f, -0.16f),
                new Vector2(0.18f, 0.4f), coat, 35);
            CreatePart(sprite, armFrontLower, "ArmFrontLowerVisual", new Vector2(0f, -0.14f),
                new Vector2(0.15f, 0.34f), skin, 36);
            CreatePart(sprite, head, "FaceVisual", Vector2.zero,
                new Vector2(0.52f, 0.58f), skin, 32);
            CreatePart(sprite, hairFront, "HairFrontVisual", new Vector2(0.02f, 0.16f),
                new Vector2(0.58f, 0.28f), hair, 40, -4f);
            CreatePart(sprite, hairFront, "SideLockVisual", new Vector2(0.24f, -0.1f),
                new Vector2(0.13f, 0.5f), hair, 41, -8f);
            CreatePart(sprite, head, "EyeVisual", new Vector2(0.15f, 0.02f),
                new Vector2(0.07f, 0.06f), new Color(0.2f, 0.75f, 0.95f), 42);
        }

        private static void BuildWeapon(Sprite sprite, Transform weaponFeedbackPivot)
        {
            Transform visualRoot = CreateNode(
                weaponFeedbackPivot,
                "WeaponVisualRoot",
                Vector2.zero);
            SpriteRenderer renderer = visualRoot.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(0.85f, 0.92f, 1f);
            renderer.sortingOrder = 60;
            WeaponVisual2D weaponVisual = visualRoot.gameObject.AddComponent<WeaponVisual2D>();
            SerializedObject serialized = new SerializedObject(weaponVisual);
            serialized.FindProperty("visualRoot").objectReferenceValue = visualRoot;
            serialized.FindProperty("spriteRenderer").objectReferenceValue = renderer;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Transform CreateNode(Transform parent, string name, Vector2 localPosition)
        {
            var node = new GameObject(name);
            Transform transform = node.transform;
            transform.SetParent(parent, false);
            transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
            return transform;
        }

        private static void CreatePart(
            Sprite sprite,
            Transform parent,
            string name,
            Vector2 localPosition,
            Vector2 size,
            Color color,
            int sortingOrder,
            float rotation = 0f)
        {
            Transform part = CreateNode(parent, name, localPosition);
            part.localRotation = Quaternion.Euler(0f, 0f, rotation);
            part.localScale = new Vector3(size.x, size.y, 1f);
            SpriteRenderer renderer = part.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
        }

        private static CharacterAppearanceDefinition BuildPresentationAssets(Sprite sprite)
        {
            WeaponPresentationDefinition sword = CreateWeaponPresentation(
                "PrototypeSwordPresentation",
                "Assets/Game/Data/Weapons/PrototypeSword.asset",
                sprite,
                new Color(0.82f, 0.92f, 1f),
                new Vector2(0.05f, -0.38f),
                -8f,
                new Vector2(0.12f, 0.9f));
            WeaponPresentationDefinition daggers = CreateWeaponPresentation(
                "PrototypeDualDaggersPresentation",
                "Assets/Game/Data/Weapons/PrototypeDualDaggers.asset",
                sprite,
                new Color(0.48f, 0.95f, 0.85f),
                new Vector2(0.04f, -0.24f),
                -22f,
                new Vector2(0.1f, 0.55f));
            WeaponPresentationDefinition hammer = CreateWeaponPresentation(
                "PrototypeHammerPresentation",
                "Assets/Game/Data/Weapons/PrototypeHammer.asset",
                sprite,
                new Color(1f, 0.62f, 0.25f),
                new Vector2(0.02f, -0.3f),
                10f,
                new Vector2(0.28f, 0.72f));

            CharacterAppearanceDefinition appearance =
                AssetDatabase.LoadAssetAtPath<CharacterAppearanceDefinition>(AppearancePath);
            if (appearance == null)
            {
                appearance = ScriptableObject.CreateInstance<CharacterAppearanceDefinition>();
                AssetDatabase.CreateAsset(appearance, AppearancePath);
            }

            SerializedObject serialized = new SerializedObject(appearance);
            serialized.FindProperty("useModelByDefault").boolValue = true;
            SerializedProperty presentations = serialized.FindProperty("weaponPresentations");
            presentations.arraySize = 3;
            presentations.GetArrayElementAtIndex(0).objectReferenceValue = sword;
            presentations.GetArrayElementAtIndex(1).objectReferenceValue = daggers;
            presentations.GetArrayElementAtIndex(2).objectReferenceValue = hammer;

            var attacks = new[]
            {
                new AttackBindingAsset(
                    "Assets/Game/Data/Combat/PrototypeBasicAttack.asset",
                    "SwordAttack1", 0.3f, 0.62f),
                new AttackBindingAsset(
                    "Assets/Game/Data/Weapons/PrototypeSwordCombo02Attack.asset",
                    "SwordAttack2", 0.28f, 0.62f),
                new AttackBindingAsset(
                    "Assets/Game/Data/Weapons/PrototypeSwordCombo03Attack.asset",
                    "SwordAttack3", 0.38f, 0.68f),
                new AttackBindingAsset(
                    "Assets/Game/Data/Weapons/PrototypeSwordSkillAttack.asset",
                    "SwordSkill", 0.34f, 0.72f),
                new AttackBindingAsset(
                    "Assets/Game/Data/Weapons/PrototypeSwordQteAttack.asset",
                    "SwordQte", 0.22f, 0.7f)
            };
            SerializedProperty bindings = serialized.FindProperty("attackAnimations");
            bindings.arraySize = attacks.Length;
            for (int index = 0; index < attacks.Length; index++)
            {
                AttackBindingAsset binding = attacks[index];
                SerializedProperty element = bindings.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("attack").objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<AttackDefinition>(binding.AssetPath);
                element.FindPropertyRelative("stateName").stringValue = binding.StateName;
                element.FindPropertyRelative("windupEndNormalized").floatValue = binding.WindupEnd;
                element.FindPropertyRelative("activeEndNormalized").floatValue = binding.ActiveEnd;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(appearance);
            return appearance;
        }

        private static WeaponPresentationDefinition CreateWeaponPresentation(
            string assetName,
            string weaponPath,
            Sprite sprite,
            Color color,
            Vector2 localPosition,
            float localRotation,
            Vector2 localScale)
        {
            string path = WeaponPresentationDirectory + "/" + assetName + ".asset";
            WeaponPresentationDefinition definition =
                AssetDatabase.LoadAssetAtPath<WeaponPresentationDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<WeaponPresentationDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("weapon").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<WeaponDefinition>(weaponPath);
            serialized.FindProperty("sprite").objectReferenceValue = sprite;
            serialized.FindProperty("color").colorValue = color;
            serialized.FindProperty("localPosition").vector2Value = localPosition;
            serialized.FindProperty("localRotation").floatValue = localRotation;
            serialized.FindProperty("localScale").vector2Value = localScale;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static void WireScene(
            GameObject modelPrefab,
            CharacterAppearanceDefinition appearance)
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            GameObject player = FindSceneObject(scene, "PlayerPrototype");
            if (player == null)
            {
                throw new InvalidOperationException("CombatSandbox player root is missing.");
            }

            Transform existingModel = player.transform.Find("PlayerModelVerticalSlice");
            if (existingModel != null)
            {
                UnityEngine.Object.DestroyImmediate(existingModel.gameObject);
            }

            GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab, scene);
            modelInstance.transform.SetParent(player.transform, false);
            modelInstance.name = "PlayerModelVerticalSlice";

            CharacterModelView2D modelView =
                GetOrAddAuthoringComponent<CharacterModelView2D>(player);
            CharacterAnimationPresenter2D presenter =
                GetOrAddAuthoringComponent<CharacterAnimationPresenter2D>(player);

            Transform facingRoot = modelInstance.transform.Find("FacingRoot");
            Transform weaponFeedbackPivot = modelInstance.transform.Find(
                "FacingRoot/SkeletonRoot/Hips/Torso/ArmFrontUpper/ArmFrontLower/" +
                "HandFront/MainHandSocket/WeaponFeedbackPivot");
            WeaponVisual2D weaponVisual =
                weaponFeedbackPivot.GetComponentInChildren<WeaponVisual2D>(true);
            Animator animator = modelInstance.GetComponent<Animator>();

            SerializedObject modelSerialized = new SerializedObject(modelView);
            SerializedProperty whiteboxes = modelSerialized.FindProperty("whiteboxObjects");
            whiteboxes.arraySize = 2;
            whiteboxes.GetArrayElementAtIndex(0).objectReferenceValue =
                player.transform.Find("Visual")?.gameObject;
            whiteboxes.GetArrayElementAtIndex(1).objectReferenceValue =
                player.transform.Find("VisualRig")?.gameObject;
            modelSerialized.FindProperty("modelRoot").objectReferenceValue = modelInstance;
            modelSerialized.FindProperty("facingRoot").objectReferenceValue = facingRoot;
            modelSerialized.FindProperty("feedbackRoot").objectReferenceValue = modelInstance.transform;
            modelSerialized.FindProperty("weaponFeedbackPivot").objectReferenceValue =
                weaponFeedbackPivot;
            modelSerialized.FindProperty("animator").objectReferenceValue = animator;
            modelSerialized.FindProperty("weaponVisual").objectReferenceValue = weaponVisual;
            modelSerialized.FindProperty("artworkFacesRight").boolValue = false;
            modelSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject presenterSerialized = new SerializedObject(presenter);
            AssignComponent<PlayerMovementController>(presenterSerialized, "movementController", player);
            AssignComponent<PlayerRollController>(presenterSerialized, "rollController", player);
            AssignComponent<PlayerAttackRunner>(presenterSerialized, "attackRunner", player);
            AssignComponent<PlayerWeaponLoadout>(presenterSerialized, "weaponLoadout", player);
            AssignComponent<PlayerWeaponSkillRunner>(presenterSerialized, "skillRunner", player);
            AssignComponent<PlayerWeaponQteExecutor>(presenterSerialized, "qteExecutor", player);
            AssignComponent<CombatReactionReceiver>(presenterSerialized, "reactionReceiver", player);
            AssignComponent<HealthComponent>(presenterSerialized, "health", player);
            presenterSerialized.FindProperty("modelView").objectReferenceValue = modelView;
            presenterSerialized.FindProperty("appearance").objectReferenceValue = appearance;
            presenterSerialized.ApplyModifiedPropertiesWithoutUndo();

            CombatAttackRecoil2D recoil = player.GetComponent<CombatAttackRecoil2D>();
            if (recoil != null)
            {
                SerializedObject recoilSerialized = new SerializedObject(recoil);
                recoilSerialized.FindProperty("modelView").objectReferenceValue = modelView;
                recoilSerialized.ApplyModifiedPropertiesWithoutUndo();
            }

            CombatHitFlash2D hitFlash = player.GetComponent<CombatHitFlash2D>();
            if (hitFlash != null)
            {
                AddModelRenderersToHitFlash(hitFlash, modelInstance);
            }

            // Keep the proven whitebox visible while authoring. The presenter enables
            // the model from CharacterAppearanceDefinition during runtime startup.
            modelInstance.SetActive(false);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static T GetOrAddAuthoringComponent<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        private static void AssignComponent<T>(
            SerializedObject serialized,
            string propertyName,
            GameObject source) where T : Component
        {
            serialized.FindProperty(propertyName).objectReferenceValue = source.GetComponent<T>();
        }

        private static void AddModelRenderersToHitFlash(
            CombatHitFlash2D hitFlash,
            GameObject modelInstance)
        {
            SerializedObject serialized = new SerializedObject(hitFlash);
            SerializedProperty renderers = serialized.FindProperty("renderers");
            var combined = new List<SpriteRenderer>();
            for (int index = 0; index < renderers.arraySize; index++)
            {
                SpriteRenderer renderer =
                    renderers.GetArrayElementAtIndex(index).objectReferenceValue as SpriteRenderer;
                if (renderer != null)
                {
                    combined.Add(renderer);
                }
            }

            SpriteRenderer[] modelRenderers =
                modelInstance.GetComponentsInChildren<SpriteRenderer>(true);
            for (int index = 0; index < modelRenderers.Length; index++)
            {
                if (!combined.Contains(modelRenderers[index]))
                {
                    combined.Add(modelRenderers[index]);
                }
            }

            renderers.arraySize = combined.Count;
            for (int index = 0; index < combined.Count; index++)
            {
                renderers.GetArrayElementAtIndex(index).objectReferenceValue = combined[index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject FindSceneObject(Scene scene, string name)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int index = 0; index < roots.Length; index++)
            {
                Transform match = FindTransform(roots[index].transform, name);
                if (match != null)
                {
                    return match.gameObject;
                }
            }

            return null;
        }

        private static Transform FindTransform(Transform current, string name)
        {
            if (current.name == name)
            {
                return current;
            }

            for (int index = 0; index < current.childCount; index++)
            {
                Transform match = FindTransform(current.GetChild(index), name);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static AnimationClip BuildIdleClip()
        {
            AnimationClip clip = PrepareClip("Idle", true);
            SetCurve(clip, SkeletonPath, "m_LocalPosition.y", Keys(0f, 0f, 0.5f, 0.035f, 1f, 0f));
            SetCurve(clip, TorsoPath, "localEulerAnglesRaw.z", Keys(0f, -1.5f, 0.5f, 1.5f, 1f, -1.5f));
            SetCurve(clip, HairFrontPath, "localEulerAnglesRaw.z", Keys(0f, -2f, 0.5f, 3f, 1f, -2f));
            SetCurve(clip, CapePath, "localEulerAnglesRaw.z", Keys(0f, 4f, 0.5f, 10f, 1f, 4f));
            return clip;
        }

        private static AnimationClip BuildRunClip()
        {
            AnimationClip clip = PrepareClip("Run", true);
            SetCurve(clip, HipsPath, "m_LocalPosition.y", Keys(0f, -0.05f, 0.25f, 0f, 0.5f, -0.05f, 0.75f, 0f, 1f, -0.05f));
            SetCurve(clip, TorsoPath, "localEulerAnglesRaw.z", Keys(0f, -8f, 0.5f, -4f, 1f, -8f));
            SetCurve(clip, ArmFrontUpperPath, "localEulerAnglesRaw.z", Keys(0f, 32f, 0.5f, -35f, 1f, 32f));
            SetCurve(clip, ArmBackUpperPath, "localEulerAnglesRaw.z", Keys(0f, -35f, 0.5f, 32f, 1f, -35f));
            SetCurve(clip, LegFrontUpperPath, "localEulerAnglesRaw.z", Keys(0f, -28f, 0.5f, 32f, 1f, -28f));
            SetCurve(clip, LegBackUpperPath, "localEulerAnglesRaw.z", Keys(0f, 32f, 0.5f, -28f, 1f, 32f));
            SetCurve(clip, CapePath, "localEulerAnglesRaw.z", Keys(0f, 22f, 0.5f, 34f, 1f, 22f));
            return clip;
        }

        private static AnimationClip BuildJumpClip()
        {
            AnimationClip clip = PrepareClip("Jump", false);
            SetCurve(clip, TorsoPath, "localEulerAnglesRaw.z", Keys(0f, -7f, 1f, -7f));
            SetCurve(clip, ArmFrontUpperPath, "localEulerAnglesRaw.z", Keys(0f, -42f, 1f, -42f));
            SetCurve(clip, ArmBackUpperPath, "localEulerAnglesRaw.z", Keys(0f, 26f, 1f, 26f));
            SetCurve(clip, LegFrontUpperPath, "localEulerAnglesRaw.z", Keys(0f, -26f, 1f, -26f));
            SetCurve(clip, LegBackUpperPath, "localEulerAnglesRaw.z", Keys(0f, 38f, 1f, 38f));
            return clip;
        }

        private static AnimationClip BuildFallClip()
        {
            AnimationClip clip = PrepareClip("Fall", true);
            SetCurve(clip, TorsoPath, "localEulerAnglesRaw.z", Keys(0f, 5f, 1f, 5f));
            SetCurve(clip, ArmFrontUpperPath, "localEulerAnglesRaw.z", Keys(0f, -70f, 0.5f, -62f, 1f, -70f));
            SetCurve(clip, ArmBackUpperPath, "localEulerAnglesRaw.z", Keys(0f, 55f, 0.5f, 48f, 1f, 55f));
            SetCurve(clip, CapePath, "localEulerAnglesRaw.z", Keys(0f, -12f, 0.5f, -4f, 1f, -12f));
            return clip;
        }

        private static AnimationClip BuildLandClip()
        {
            AnimationClip clip = PrepareClip("Land", false);
            SetCurve(clip, SkeletonPath, "m_LocalScale.x", Keys(0f, 1.15f, 0.45f, 0.96f, 1f, 1f));
            SetCurve(clip, SkeletonPath, "m_LocalScale.y", Keys(0f, 0.72f, 0.45f, 1.05f, 1f, 1f));
            SetCurve(clip, HipsPath, "m_LocalPosition.y", Keys(0f, -0.18f, 1f, -0.05f));
            return clip;
        }

        private static AnimationClip BuildRollClip()
        {
            AnimationClip clip = PrepareClip("Roll", false);
            SetCurve(clip, SkeletonPath, "localEulerAnglesRaw.z", Keys(0f, 0f, 1f, -360f));
            SetCurve(clip, SkeletonPath, "m_LocalScale.x", Keys(0f, 1f, 0.5f, 0.82f, 1f, 1f));
            SetCurve(clip, SkeletonPath, "m_LocalScale.y", Keys(0f, 1f, 0.5f, 0.82f, 1f, 1f));
            return clip;
        }

        private static AnimationClip BuildHurtClip()
        {
            AnimationClip clip = PrepareClip("Hurt", false);
            SetCurve(clip, SkeletonPath, "localEulerAnglesRaw.z", Keys(0f, 0f, 0.15f, 16f, 0.5f, -8f, 1f, 0f));
            SetCurve(clip, ArmFrontUpperPath, "localEulerAnglesRaw.z", Keys(0f, 0f, 0.2f, 38f, 1f, 12f));
            return clip;
        }

        private static AnimationClip BuildControlledClip()
        {
            AnimationClip clip = PrepareClip("Controlled", true);
            SetCurve(clip, SkeletonPath, "localEulerAnglesRaw.z", Keys(0f, 18f, 0.5f, 24f, 1f, 18f));
            SetCurve(clip, ArmFrontUpperPath, "localEulerAnglesRaw.z", Keys(0f, 55f, 0.5f, 64f, 1f, 55f));
            SetCurve(clip, ArmBackUpperPath, "localEulerAnglesRaw.z", Keys(0f, -48f, 0.5f, -56f, 1f, -48f));
            return clip;
        }

        private static AnimationClip BuildDeadClip()
        {
            AnimationClip clip = PrepareClip("Dead", false);
            SetCurve(clip, SkeletonPath, "localEulerAnglesRaw.z", Keys(0f, 0f, 0.6f, -82f, 1f, -90f));
            SetCurve(clip, SkeletonPath, "m_LocalPosition.y", Keys(0f, 0f, 1f, -0.55f));
            return clip;
        }

        private static AnimationClip BuildQteApproachClip()
        {
            AnimationClip clip = PrepareClip("QteApproach", true);
            SetCurve(clip, TorsoPath, "localEulerAnglesRaw.z", Keys(0f, -18f, 1f, -18f));
            SetCurve(clip, HipsPath, "m_LocalPosition.y", Keys(0f, -0.05f, 0.5f, 0.02f, 1f, -0.05f));
            SetCurve(clip, CapePath, "localEulerAnglesRaw.z", Keys(0f, 35f, 0.5f, 45f, 1f, 35f));
            return clip;
        }

        private static AnimationClip BuildSwordAttack1Clip(string name)
        {
            AnimationClip clip = PrepareClip(name, false);
            SetCurve(clip, TorsoPath, "localEulerAnglesRaw.z", Keys(0f, -10f, 0.3f, -18f, 0.62f, 16f, 1f, 0f));
            SetCurve(clip, ArmFrontUpperPath, "localEulerAnglesRaw.z", Keys(0f, 58f, 0.3f, 82f, 0.62f, -86f, 1f, 0f));
            SetCurve(clip, ArmFrontLowerPath, "localEulerAnglesRaw.z", Keys(0f, -28f, 0.3f, -42f, 0.62f, 12f, 1f, 0f));
            return clip;
        }

        private static AnimationClip BuildSwordAttack2Clip()
        {
            AnimationClip clip = PrepareClip("SwordAttack2", false);
            SetCurve(clip, TorsoPath, "localEulerAnglesRaw.z", Keys(0f, 12f, 0.28f, 20f, 0.62f, -16f, 1f, 0f));
            SetCurve(clip, ArmFrontUpperPath, "localEulerAnglesRaw.z", Keys(0f, -82f, 0.28f, -102f, 0.62f, 68f, 1f, 0f));
            SetCurve(clip, ArmFrontLowerPath, "localEulerAnglesRaw.z", Keys(0f, 18f, 0.28f, 35f, 0.62f, -16f, 1f, 0f));
            return clip;
        }

        private static AnimationClip BuildSwordAttack3Clip()
        {
            AnimationClip clip = PrepareClip("SwordAttack3", false);
            SetCurve(clip, TorsoPath, "localEulerAnglesRaw.z", Keys(0f, -12f, 0.38f, -24f, 0.68f, 22f, 1f, 0f));
            SetCurve(clip, ArmFrontUpperPath, "localEulerAnglesRaw.z", Keys(0f, 25f, 0.38f, 152f, 0.68f, -35f, 1f, 0f));
            SetCurve(clip, ArmFrontLowerPath, "localEulerAnglesRaw.z", Keys(0f, -15f, 0.38f, -28f, 0.68f, 8f, 1f, 0f));
            SetCurve(clip, SkeletonPath, "m_LocalPosition.y", Keys(0f, 0f, 0.38f, 0.08f, 0.68f, -0.06f, 1f, 0f));
            return clip;
        }

        private static AnimationClip BuildSwordSkillClip()
        {
            AnimationClip clip = PrepareClip("SwordSkill", false);
            SetCurve(clip, SkeletonPath, "localEulerAnglesRaw.z", Keys(0f, 0f, 0.34f, -80f, 0.72f, 280f, 1f, 360f));
            SetCurve(clip, ArmFrontUpperPath, "localEulerAnglesRaw.z", Keys(0f, 80f, 1f, 80f));
            SetCurve(clip, CapePath, "localEulerAnglesRaw.z", Keys(0f, 20f, 0.5f, 55f, 1f, 20f));
            return clip;
        }

        private static AnimationClip BuildSwordQteClip()
        {
            AnimationClip clip = PrepareClip("SwordQte", false);
            SetCurve(clip, TorsoPath, "localEulerAnglesRaw.z", Keys(0f, -22f, 0.22f, -30f, 0.7f, 18f, 1f, 0f));
            SetCurve(clip, ArmFrontUpperPath, "localEulerAnglesRaw.z", Keys(0f, 108f, 0.22f, 125f, 0.7f, -98f, 1f, 0f));
            SetCurve(clip, SkeletonPath, "m_LocalPosition.x", Keys(0f, -0.08f, 0.22f, -0.14f, 0.7f, 0.2f, 1f, 0f));
            return clip;
        }

        private static AnimationClip PrepareClip(string name, bool loop)
        {
            string path = ClipDirectory + "/" + name + ".anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip { name = name, frameRate = 60f };
                AssetDatabase.CreateAsset(clip, path);
            }

            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
            for (int index = 0; index < bindings.Length; index++)
            {
                AnimationUtility.SetEditorCurve(clip, bindings[index], null);
            }

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static void SetCurve(
            AnimationClip clip,
            string path,
            string property,
            Keyframe[] keys)
        {
            AnimationUtility.SetEditorCurve(
                clip,
                EditorCurveBinding.FloatCurve(path, typeof(Transform), property),
                new AnimationCurve(keys));
        }

        private static Keyframe[] Keys(params float[] timeValuePairs)
        {
            if (timeValuePairs == null || timeValuePairs.Length % 2 != 0)
            {
                throw new ArgumentException("Animation keys require time/value pairs.");
            }

            var keys = new Keyframe[timeValuePairs.Length / 2];
            for (int index = 0; index < keys.Length; index++)
            {
                keys[index] = new Keyframe(
                    timeValuePairs[index * 2],
                    timeValuePairs[index * 2 + 1]);
            }

            return keys;
        }

        private readonly struct AttackBindingAsset
        {
            internal AttackBindingAsset(
                string assetPath,
                string stateName,
                float windupEnd,
                float activeEnd)
            {
                AssetPath = assetPath;
                StateName = stateName;
                WindupEnd = windupEnd;
                ActiveEnd = activeEnd;
            }

            internal string AssetPath { get; }
            internal string StateName { get; }
            internal float WindupEnd { get; }
            internal float ActiveEnd { get; }
        }
    }
}
