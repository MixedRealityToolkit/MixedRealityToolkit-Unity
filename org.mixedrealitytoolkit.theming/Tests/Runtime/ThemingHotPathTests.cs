// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

#pragma warning disable CS1591

using MixedReality.Toolkit.Core.Tests;
using MixedReality.Toolkit.UX;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Constraints;
using Debug = UnityEngine.Debug;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace MixedReality.Toolkit.Theming.Tests.Runtime
{
    /// <summary>
    /// Runtime performance and regression tests verifying lookup latency,
    /// cache correctness, delegate caching, and Zero-GC invariants across Theming hot paths.
    /// </summary>
    public class ThemingHotPathTests : BaseRuntimeTests
    {
        private class TestBinder : BaseThemeBinder<bool, Behaviour>
        {
            public bool AppliedValue { get; private set; }
            public int ApplyCount { get; private set; }

            public void Initialize(string itemName, Behaviour target)
            {
                PropertyInfo nameProp = typeof(BaseThemeBinder<bool, Behaviour>).GetProperty(
                    "ThemeDefinitionItemName",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                nameProp?.GetSetMethod(true)?.Invoke(this, new object[] { itemName });

                PropertyInfo targetProp = typeof(BaseThemeBinder<bool, Behaviour>).GetProperty(
                    "Target",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                targetProp?.GetSetMethod(true)?.Invoke(this, new object[] { target });
            }

            protected override void Apply(BaseThemeItemData<bool> themeItemData)
            {
                if (themeItemData != null)
                {
                    AppliedValue = themeItemData.Value;
                    ApplyCount++;
                }
            }
        }

        private static Theme CreateTestTheme(int totalItemCount = 100)
        {
            Theme theme = ScriptableObject.CreateInstance<Theme>();
            FieldInfo themeItemsField = typeof(Theme).GetField("themeItems", BindingFlags.NonPublic | BindingFlags.Instance);
            List<Theme.ThemeItem> items = (List<Theme.ThemeItem>)themeItemsField.GetValue(theme);
            if (items == null)
            {
                items = new List<Theme.ThemeItem>();
                themeItemsField.SetValue(theme, items);
            }

            items.Add(new Theme.ThemeItem("PrimaryColor", new ColorThemeItemData { Value = Color.red }));
            items.Add(new Theme.ThemeItem("SecondaryColor", new ColorThemeItemData { Value = Color.blue }));
            items.Add(new Theme.ThemeItem("IsEnabled", new BoolItemData { Value = true }));
            items.Add(new Theme.ThemeItem("FontSize", new FloatItemData { Value = 14.5f }));
            items.Add(new Theme.ThemeItem("ScaleVector", new Vector3ItemData { Value = Vector3.one * 2f }));

            for (int i = 5; i < totalItemCount; i++)
            {
                items.Add(new Theme.ThemeItem($"Item_{i}", new FloatItemData { Value = i * 1.5f }));
            }

            return theme;
        }

        [UnityTest]
        public IEnumerator Theme_TryGetItemData_AccuracyAndTypes()
        {
            Theme theme = CreateTestTheme(20);

            // 1. Correct typed retrieval
            Assert.IsTrue(theme.TryGetItemData("PrimaryColor", out ColorThemeItemData colorData));
            Assert.AreEqual(Color.red, colorData.Value);

            Assert.IsTrue(theme.TryGetItemData("IsEnabled", out BoolItemData boolData));
            Assert.IsTrue(boolData.Value);

            Assert.IsTrue(theme.TryGetItemData("FontSize", out FloatItemData floatData));
            Assert.AreEqual(14.5f, floatData.Value);

            Assert.IsTrue(theme.TryGetItemData("ScaleVector", out Vector3ItemData vecData));
            Assert.AreEqual(Vector3.one * 2f, vecData.Value);

            // 2. Type mismatch returns false
            Assert.IsFalse(theme.TryGetItemData("PrimaryColor", out BoolItemData _));

            // 3. Nonexistent item returns false
            Assert.IsFalse(theme.TryGetItemData("NonExistentItem", out ColorThemeItemData _));

            // 4. Null and empty keys return false without throwing
            Assert.IsFalse(theme.TryGetItemData(null, out ColorThemeItemData _));
            Assert.IsFalse(theme.TryGetItemData(string.Empty, out ColorThemeItemData _));

            Object.Destroy(theme);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Theme_TryGetItemData_BenchmarkAndAllocation()
        {
            const int totalItems = 100;
            const int lookupIterations = 50000;
            Theme theme = CreateTestTheme(totalItems);

            // Warmup
            for (int i = 0; i < 500; i++)
            {
                theme.TryGetItemData("PrimaryColor", out ColorThemeItemData _);
                theme.TryGetItemData("Item_50", out FloatItemData _);
                theme.TryGetItemData("Item_99", out FloatItemData _);
            }

            // Benchmark
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < lookupIterations; i++)
            {
                theme.TryGetItemData("PrimaryColor", out ColorThemeItemData _);
                theme.TryGetItemData("Item_50", out FloatItemData _);
                theme.TryGetItemData("Item_99", out FloatItemData _);
                theme.TryGetItemData("NonExistent", out FloatItemData _);
            }
            sw.Stop();
            long totalLookups = (long)lookupIterations * 4;
            double msPerLookup = (double)sw.ElapsedTicks / Stopwatch.Frequency * 1000.0 / totalLookups;
            Debug.Log($"[Theming Perf] {totalLookups} lookups in {sw.ElapsedMilliseconds} ms ({msPerLookup * 1000.0:F3} us/lookup) for a {totalItems}-item theme.");

            // Zero-GC verification for steady-state lookups
            TestDelegate lookupAction = () =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    theme.TryGetItemData("PrimaryColor", out ColorThemeItemData _);
                    theme.TryGetItemData("Item_50", out FloatItemData _);
                    theme.TryGetItemData("Item_99", out FloatItemData _);
                    theme.TryGetItemData("NonExistent", out FloatItemData _);
                }
            };
            lookupAction();
            Assert.That(lookupAction, Is.Not.AllocatingGCMemory());

            Object.Destroy(theme);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Theme_CacheInvalidation_ReflectsUpdates()
        {
            Theme theme = CreateTestTheme(20);

            Assert.IsTrue(theme.TryGetItemData("Item_10", out FloatItemData initialData));
            Assert.AreEqual(15.0f, initialData.Value);

            // Mutate item value in list
            FieldInfo themeItemsField = typeof(Theme).GetField("themeItems", BindingFlags.NonPublic | BindingFlags.Instance);
            List<Theme.ThemeItem> items = (List<Theme.ThemeItem>)themeItemsField.GetValue(theme);
            int idx = items.FindIndex(it => it.Name == "Item_10");
            items[idx] = new Theme.ThemeItem("Item_10", new FloatItemData { Value = 999.0f });

            // Invalidate cache if method exists
            MethodInfo invalidateMethod = typeof(Theme).GetMethod("InvalidateCache", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            invalidateMethod?.Invoke(theme, null);

            Assert.IsTrue(theme.TryGetItemData("Item_10", out FloatItemData updatedData));
            Assert.AreEqual(999.0f, updatedData.Value);

            Object.Destroy(theme);
            yield return null;
        }

        [UnityTest]
        public IEnumerator BaseThemeBinder_SubscribeUnsubscribe_AllocationTest()
        {
            ThemeDataSource dataSource = ScriptableObject.CreateInstance<ThemeDataSource>();
            Theme theme = CreateTestTheme(10);
            dataSource.value = theme;

            GameObject go = new GameObject("TestBinderTarget");
            Behaviour dummyBehaviour = go.AddComponent<Light>();
            TestBinder binder = new TestBinder();
            binder.Initialize("IsEnabled", dummyBehaviour);

            IBinder iBinder = binder;

            // Warmup initial subscription
            iBinder.Subscribe(dataSource);
            iBinder.Unsubscribe(dataSource);

            TestDelegate subscribeCycle = () =>
            {
                for (int i = 0; i < 50; i++)
                {
                    iBinder.Subscribe(dataSource);
                    iBinder.Unsubscribe(dataSource);
                }
            };
            subscribeCycle();
            Assert.That(subscribeCycle, Is.Not.AllocatingGCMemory());

            Object.Destroy(go);
            Object.Destroy(theme);
            Object.Destroy(dataSource);
            yield return null;
        }

        [UnityTest]
        public IEnumerator BaseThemeBinder_ThemeChange_AppliesNewValue()
        {
            ThemeDataSource dataSource = ScriptableObject.CreateInstance<ThemeDataSource>();

            Theme themeA = CreateTestTheme(10);
            FieldInfo themeItemsField = typeof(Theme).GetField("themeItems", BindingFlags.NonPublic | BindingFlags.Instance);
            List<Theme.ThemeItem> itemsA = (List<Theme.ThemeItem>)themeItemsField.GetValue(themeA);
            int idxA = itemsA.FindIndex(it => it.Name == "IsEnabled");
            itemsA[idxA] = new Theme.ThemeItem("IsEnabled", new BoolItemData { Value = true });

            Theme themeB = CreateTestTheme(10);
            List<Theme.ThemeItem> itemsB = (List<Theme.ThemeItem>)themeItemsField.GetValue(themeB);
            int idxB = itemsB.FindIndex(it => it.Name == "IsEnabled");
            itemsB[idxB] = new Theme.ThemeItem("IsEnabled", new BoolItemData { Value = false });

            dataSource.value = themeA;

            GameObject go = new GameObject("TestBinderTarget");
            Behaviour dummyBehaviour = go.AddComponent<Light>();
            TestBinder binder = new TestBinder();
            binder.Initialize("IsEnabled", dummyBehaviour);

            IBinder iBinder = binder;
            iBinder.Subscribe(dataSource);

            // AddListener should immediately trigger apply for activeTheme
            Assert.AreEqual(1, binder.ApplyCount);
            Assert.IsTrue(binder.AppliedValue);

            // Switch to themeB
            dataSource.value = themeB;
            Assert.AreEqual(2, binder.ApplyCount);
            Assert.IsFalse(binder.AppliedValue);

            // Unsubscribe and switch back to themeA; binder should not receive it
            iBinder.Unsubscribe(dataSource);
            dataSource.value = themeA;
            Assert.AreEqual(2, binder.ApplyCount);
            Assert.IsFalse(binder.AppliedValue);

            Object.Destroy(go);
            Object.Destroy(themeA);
            Object.Destroy(themeB);
            Object.Destroy(dataSource);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ThemeBinding_Lifecycle_ZeroGC()
        {
            ThemeDataSource dataSource = ScriptableObject.CreateInstance<ThemeDataSource>();
            Theme theme = CreateTestTheme(10);
            dataSource.value = theme;

            GameObject go = new GameObject("ThemeBindingHolder");
            go.SetActive(false);
            Behaviour dummyBehaviour = go.AddComponent<Light>();
            ThemeBinding themeBinding = go.AddComponent<ThemeBinding>();

            TestBinder binder = new TestBinder();
            binder.Initialize("IsEnabled", dummyBehaviour);

            FieldInfo dataSourceField = typeof(ThemeBinding).GetField("themeDataSource", BindingFlags.NonPublic | BindingFlags.Instance);
            dataSourceField?.SetValue(themeBinding, dataSource);

            FieldInfo bindersField = typeof(ThemeBinding).GetField("binders", BindingFlags.NonPublic | BindingFlags.Instance);
            bindersField?.SetValue(themeBinding, new IBinder[] { binder });

            // Warmup initial enable/disable
            go.SetActive(true);
            go.SetActive(false);

            // Lifecycle Zero-GC test across repeated disable/enable
            TestDelegate lifecycleCycle = () =>
            {
                for (int i = 0; i < 20; i++)
                {
                    go.SetActive(true);
                    go.SetActive(false);
                }
            };
            lifecycleCycle();
            Assert.That(lifecycleCycle, Is.Not.AllocatingGCMemory());

            Object.Destroy(go);
            Object.Destroy(theme);
            Object.Destroy(dataSource);
            yield return null;
        }
    }
}
