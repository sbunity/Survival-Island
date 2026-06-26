using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using UnityEditor;

namespace Watermelon.Tests
{
    [TestFixture]
    public class UIPageContractTests
    {
        // call (0x28) and callvirt (0x6F) — the only IL opcodes that invoke methods
        private const byte OP_CALL = 0x28;
        private const byte OP_CALLVIRT = 0x6F;

        private static readonly Type CompilerGeneratedAttr = typeof(CompilerGeneratedAttribute);

        private static readonly BindingFlags AllDeclared =
            BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.DeclaredOnly;

        private static readonly MethodInfo NotifyOpenedMethod =
            typeof(UIPage).GetMethod("NotifyOpened", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly MethodInfo NotifyClosedMethod =
            typeof(UIPage).GetMethod("NotifyClosed", BindingFlags.NonPublic | BindingFlags.Instance);

        private static IEnumerable<TestCaseData> ConcretePageTypes()
        {
            foreach (var type in TypeCache.GetTypesDerivedFrom<UIPage>())
            {
                if (!type.IsAbstract)
                    yield return new TestCaseData(type).SetName(type.Name);
            }
        }

        [TestCaseSource(nameof(ConcretePageTypes))]
        public void OnShow_CallsNotifyOpened(Type pageType)
        {
            var onShow = FindOverride(pageType, "OnShow");
            Assert.IsNotNull(onShow, $"{pageType.Name}: OnShow() method not found");
            Assert.IsTrue(
                AnyILContainsCallTo(onShow, NotifyOpenedMethod),
                $"{pageType.Name}.OnShow() does not call NotifyOpened() — check direct calls and callbacks");
        }

        [TestCaseSource(nameof(ConcretePageTypes))]
        public void OnHide_CallsNotifyClosed(Type pageType)
        {
            var onHide = FindOverride(pageType, "OnHide");
            Assert.IsNotNull(onHide, $"{pageType.Name}: OnHide() method not found");
            Assert.IsTrue(
                AnyILContainsCallTo(onHide, NotifyClosedMethod),
                $"{pageType.Name}.OnHide() does not call NotifyClosed() — check direct calls and callbacks");
        }

        // Walks up the hierarchy to find the most-derived non-abstract override.
        private static MethodInfo FindOverride(Type type, string methodName)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            while (type != null && type != typeof(UIPage))
            {
                var m = type.GetMethod(methodName, flags);
                if (m != null && !m.IsAbstract)
                    return m;

                type = type.BaseType;
            }
            return null;
        }

        // Checks the method itself and all compiler-generated methods (lambdas, local
        // functions, coroutine state machines) spawned from it.
        private static bool AnyILContainsCallTo(MethodInfo method, MethodInfo target)
        {
            foreach (var m in MethodWithGeneratedCallbacks(method))
            {
                if (ILContainsCallTo(m, target))
                    return true;
            }
            return false;
        }

        // Yields the method itself plus every compiler-generated method whose name starts
        // with <MethodName> — lambdas, local functions, and iterator/async state machines
        // all follow this convention regardless of whether they capture variables.
        private static IEnumerable<MethodBase> MethodWithGeneratedCallbacks(MethodInfo method)
        {
            yield return method;

            string prefix = $"<{method.Name}>";
            Type declaringType = method.DeclaringType;

            // Lambdas that don't capture → static methods on the declaring type itself
            foreach (var m in declaringType.GetMethods(AllDeclared))
            {
                if (m.Name.StartsWith(prefix) && m.IsDefined(CompilerGeneratedAttr, false))
                    yield return m;
            }

            // Lambdas that capture variables → methods on nested display-class types
            foreach (var nested in declaringType.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (!nested.IsDefined(CompilerGeneratedAttr, false))
                    continue;

                foreach (var m in nested.GetMethods(AllDeclared))
                {
                    if (m.Name.StartsWith(prefix) && m.IsDefined(CompilerGeneratedAttr, false))
                        yield return m;
                }
            }
        }

        // Scans IL bytes for call/callvirt instructions that resolve to the target method.
        private static bool ILContainsCallTo(MethodBase caller, MethodInfo target)
        {
            var body = caller.GetMethodBody();
            if (body == null)
                return false;

            byte[] il = body.GetILAsByteArray();
            Module module = caller.Module;

            for (int i = 0; i < il.Length - 4; i++)
            {
                if (il[i] != OP_CALL && il[i] != OP_CALLVIRT)
                    continue;

                int token = BitConverter.ToInt32(il, i + 1);
                try
                {
                    if (module.ResolveMethod(token) == target)
                        return true;
                }
                catch (ArgumentException) { }
            }

            return false;
        }
    }
}
