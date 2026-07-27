using System.Reflection;
using JustTest.Game.Run;
using NUnit.Framework;
using UnityEngine;

namespace JustTest.Game.Tests.EditMode
{
    public sealed class CombatRunConfigTests
    {
        [Test]
        public void OnValidateSanitizesInvalidDelays()
        {
            CombatRunConfig config = ScriptableObject.CreateInstance<CombatRunConfig>();
            try
            {
                SetPrivateField(config, "restartInputDelayAfterResult", -1f);
                SetPrivateField(config, "sceneReloadDelay", float.NaN);

                InvokeOnValidate(config);

                Assert.That(config.RestartInputDelayAfterResult, Is.EqualTo(0f));
                Assert.That(config.SceneReloadDelay, Is.EqualTo(0f));
                Assert.That(config.IsValid, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        private static void SetPrivateField(CombatRunConfig config, string fieldName, float value)
        {
            FieldInfo field = typeof(CombatRunConfig).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(config, value);
        }

        private static void InvokeOnValidate(CombatRunConfig config)
        {
            MethodInfo method = typeof(CombatRunConfig).GetMethod(
                "OnValidate",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(config, null);
        }
    }
}
