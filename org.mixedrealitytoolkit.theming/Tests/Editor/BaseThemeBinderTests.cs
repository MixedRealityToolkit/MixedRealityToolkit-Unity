// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using NUnit.Framework;
using System;
using System.Linq;
using UnityEditor;

namespace MixedReality.Toolkit.Theming.Tests.EditMode
{
    /// <summary>
    /// Tests to evaluate the architectural integrity of Theme Binders.
    /// </summary>
    public class BaseThemeBinderTests
    {
        /// <summary>
        /// Verifies that all non-abstract theme binders have fully resolved their generic arguments (T and K).
        /// </summary>
        [Test]
        public void ConcreteBindersFullyResolveGenericArguments()
        {
            // Quickly grab every class in the project that implements IBinder
            var binderTypes = TypeCache.GetTypesDerivedFrom(typeof(IBinder))
                .Where(t => !t.IsAbstract && !t.IsInterface);

            int testedCount = 0;

            foreach (Type type in binderTypes)
            {
                Assert.IsFalse(type.ContainsGenericParameters,
                    $"Binder type '{type.Name}' is not abstract, but contains open generic parameters. " +
                    "Concrete binders must fully resolve their BaseThemeBinder<T, K> data types.");

                Type baseBinderType = type;
                while (baseBinderType != null && (!baseBinderType.IsGenericType || baseBinderType.GetGenericTypeDefinition() != typeof(BaseThemeBinder<,>)))
                {
                    baseBinderType = baseBinderType.BaseType;
                }

                if (baseBinderType != null)
                {
                    Type valueType = baseBinderType.GenericTypeArguments[0];
                    Type targetType = baseBinderType.GenericTypeArguments[1];

                    Assert.IsNotNull(valueType, $"Value type (T) for '{type.Name}' could not be resolved.");
                    Assert.IsNotNull(targetType, $"Target type (K) for '{type.Name}' could not be resolved.");

                    testedCount++;
                }
            }

            Assert.Greater(testedCount, 0, "No concrete BaseThemeBinder<T, K> implementations were found to test.");
        }

        /// <summary>
        /// Verifies that all non-abstract theme item data classes have the [Serializable] attribute.
        /// </summary>
        [Test]
        public void ConcreteThemeItemDataClassesAreSerializable()
        {
            var itemDataTypes = TypeCache.GetTypesDerivedFrom(typeof(BaseThemeItemData<>))
                .Where(t => !t.IsAbstract && !t.IsInterface);

            int testedCount = 0;
            foreach (Type type in itemDataTypes)
            {
                Assert.IsTrue(type.IsSerializable,
                    $"Theme item data type '{type.Name}' must have the [Serializable] attribute to be saved in a Theme asset.");
                testedCount++;
            }

            Assert.Greater(testedCount, 0, "No concrete BaseThemeItemData<T> implementations were found to test.");
        }

        /// <summary>
        /// Verifies that every generic data type targeted by a binder has a corresponding concrete ThemeItemData class to hold it.
        /// </summary>
        [Test]
        public void BindersHaveCorrespondingThemeItemData()
        {
            var binderTypes = TypeCache.GetTypesDerivedFrom(typeof(IBinder))
                .Where(t => !t.IsAbstract && !t.IsInterface);

            var itemDataTypes = TypeCache.GetTypesDerivedFrom(typeof(BaseThemeItemData<>))
                .Where(t => !t.IsAbstract && !t.IsInterface).ToList();

            int testedCount = 0;
            foreach (Type binderType in binderTypes)
            {
                Type baseBinderType = binderType;
                while (baseBinderType != null && (!baseBinderType.IsGenericType || baseBinderType.GetGenericTypeDefinition() != typeof(BaseThemeBinder<,>)))
                {
                    baseBinderType = baseBinderType.BaseType;
                }

                if (baseBinderType != null)
                {
                    Type valueType = baseBinderType.GenericTypeArguments[0];

                    bool hasMatchingItemData = false;
                    foreach (Type itemDataType in itemDataTypes)
                    {
                        Type baseItemDataType = itemDataType;
                        while (baseItemDataType != null && (!baseItemDataType.IsGenericType || baseItemDataType.GetGenericTypeDefinition() != typeof(BaseThemeItemData<>)))
                        {
                            baseItemDataType = baseItemDataType.BaseType;
                        }

                        if (baseItemDataType != null && baseItemDataType.GenericTypeArguments[0] == valueType)
                        {
                            hasMatchingItemData = true;
                            break;
                        }
                    }

                    Assert.IsTrue(hasMatchingItemData,
                        $"Binder '{binderType.Name}' binds to '{valueType.Name}', but no concrete BaseThemeItemData<{valueType.Name}> class exists to serialize it in the Theme asset.");
                    testedCount++;
                }
            }

            Assert.Greater(testedCount, 0, "No concrete BaseThemeBinder<T, K> implementations were found to test.");
        }
    }
}
