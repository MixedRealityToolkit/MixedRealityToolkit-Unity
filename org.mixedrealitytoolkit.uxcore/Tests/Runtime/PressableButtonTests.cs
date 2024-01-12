// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using MixedReality.Toolkit.Input.Tests;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MixedReality.Toolkit.UX.Runtime.Tests
{
    /// <summary>
    /// Tests for Pressable Button script
    /// </summary>
    public class PressableButtonTests : BaseRuntimeInputTests
    {
        GameObject buttonsContainerGameObject = new GameObject("GameObjectForButtonsTest");
        PressableButton pressableButton = null;

        /// <summary>
        /// Initialize the GameObject that stores the PressableButton for testing.
        /// </summary>
        [SetUp]
        public void Init()
        {
            pressableButton = buttonsContainerGameObject.AddComponent<PressableButton>();
        }

        /// <summary>
        /// Clean-up the GameObject that stores the PressableButton for testing.
        /// </summary>
        [TearDown]
        public void Teardown()
        {
            UnityEngine.Object.Destroy(buttonsContainerGameObject);
        }

        /// <summary>
        /// Test the script has the frontPlate field.
        /// </summary>
        [UnityTest]
        public IEnumerator PressableButton_Has_frontPlate_Field()
        {
            FieldInfo[] fieldInfos;
            Type pressableButtonType = typeof(PressableButton);

            fieldInfos = pressableButtonType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            var result = fieldInfos.Where(fieldInfo => fieldInfo.Name.Equals("frontPlate")).ToArray();

            Assert.IsTrue(result.Length == 1);

            yield return null;
        }

        /// <summary>
        /// Test the script has get and set accessor to the frontPlate field.
        /// </summary>
        [UnityTest]
        public IEnumerator PressableButton_Has_FrontPlate_Accessors()
        {
            MethodInfo[] methodInfos;
            Type pressableButtonType = typeof(PressableButton);

            methodInfos = pressableButtonType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            var getAccessors = methodInfos.Where(methodInfo => methodInfo.Name.Equals("get_FrontPlate")).ToArray();
            var setAccessors = methodInfos.Where(methodInfo => methodInfo.Name.Equals("set_FrontPlate")).ToArray();

            Assert.IsTrue(getAccessors.Length == 1);
            Assert.IsTrue(setAccessors.Length == 1);

            yield return null;
        }

        /// <summary>
        /// Test the script has the frontPlateRawImage field.
        /// </summary>
        [UnityTest]
        public IEnumerator PressableButton_Has_frontPlateRawImage_Field()
        {
            FieldInfo[] fieldInfos;
            Type pressableButtonType = typeof(PressableButton);

            fieldInfos = pressableButtonType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            var result = fieldInfos.Where(fieldInfo => fieldInfo.Name.Equals("frontPlateRawImage")).ToArray();

            Assert.IsTrue(result.Length == 1);

            yield return null;
        }

        /// <summary>
        /// Test the script has the canvasElementRoundedRect field.
        /// </summary>
        [UnityTest]
        public IEnumerator PressableButton_Has_canvasElementRoundedRect_Field()
        {
            FieldInfo[] fieldInfos;
            Type pressableButtonType = typeof(PressableButton);

            fieldInfos = pressableButtonType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            var result = fieldInfos.Where(fieldInfo => fieldInfo.Name.Equals("canvasElementRoundedRect")).ToArray();

            Assert.IsTrue(result.Length == 1);

            yield return null;
        }

        /// <summary>
        /// Test the script has get and set EnableOnHoverOnlyCanvasRoundedRect accessors.
        /// </summary>
        [UnityTest]
        public IEnumerator PressableButton_Has_EnableOnHoverOnlyCanvasRoundedRect_Accessors()
        {
            MethodInfo[] methodInfos;
            Type pressableButtonType = typeof(PressableButton);

            methodInfos = pressableButtonType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            var getAccessors = methodInfos.Where(methodInfo => methodInfo.Name.Equals("get_EnableOnHoverOnlyCanvasRoundedRect")).ToArray();
            var setAccessors = methodInfos.Where(methodInfo => methodInfo.Name.Equals("set_EnableOnHoverOnlyCanvasRoundedRect")).ToArray();

            Assert.IsTrue(getAccessors.Length == 1);
            Assert.IsTrue(setAccessors.Length == 1);

            yield return null;
        }

        /// <summary>
        /// Test the script has get and set EnableOnHoverOnlyFrontPlate accessors.
        /// </summary>
        [UnityTest]
        public IEnumerator PressableButton_Has_EnableOnHoverOnlyFrontPlateRawImage_Accessors()
        {
            MethodInfo[] methodInfos;
            Type pressableButtonType = typeof(PressableButton);

            methodInfos = pressableButtonType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            var getAccessors = methodInfos.Where(methodInfo => methodInfo.Name.Equals("get_EnableOnHoverOnlyFrontPlateRawImage")).ToArray();
            var setAccessors = methodInfos.Where(methodInfo => methodInfo.Name.Equals("set_EnableOnHoverOnlyFrontPlateRawImage")).ToArray();

            Assert.IsTrue(getAccessors.Length == 1);
            Assert.IsTrue(setAccessors.Length == 1);

            yield return null;
        }
    }
}
#pragma warning restore CS1591
