// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;

using Unity.XR.CoreUtils;
using NUnit.Framework;
using UnityEngine.XR.Interaction.Toolkit;
using MixedReality.Toolkit.Input.Experimental;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MixedReality.Toolkit.Input.Tests
{
    /// <summary>
    /// Basic tests for controllerless SpatialMouseInput.
    /// </summary>
    /// <remarks>
    /// This is an XRI3+ exclusive test.
    /// </remarks>
    public class SpatialMouseInputTestsForControllerlessRig : BaseRuntimeInputTests
    {
        private const string SpatialMouseControllerPrefabGuid = "dc525621b8522034e867ed2799129315";
        private static readonly string SpatialMouseControllerPrefabPath = AssetDatabase.GUIDToAssetPath(SpatialMouseControllerPrefabGuid);

        private static GameObject spatialMouseGameObject;

        private const string CameraOffsetName = "Camera Offset";
        private const string SpatialMouseInteractorName = "SpatialMouseInteractor";
        private const string MouseMoveName = "MouseMove";
        private const string MouseScroll = "MouseScroll";

        [UnitySetUp]
        public override IEnumerator Setup()
        {
            yield return base.Setup();
            var spatialMouseController = InstantiateSpatialMouseController();
            List<GameObject> rigChildren = new List<GameObject>();
            InputTestUtilities.RigReference.GetChildGameObjects(rigChildren);
            var cameraOffset = rigChildren.Find(go => go.name.Equals(CameraOffsetName));
            spatialMouseController.transform.parent = cameraOffset.transform;
            yield return null;
        }

        /// <summary>
        /// Verify that the SpatialMouseInteractor has the proper XRI3+ configurations.
        /// </summary>
        /// <remarks>
        /// This is an XRI3+ exclusive test.
        /// </remarks>
        [UnityTest]
        public IEnumerator SpatialMouseInteractorControllerlessSmokeTest()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            Assert.IsNull(spatialMouseGameObject.GetComponent<XRBaseController>());
#pragma warning restore CS0618 // Type or member is obsolete

            // Check the SpatialMouseController has the SpatialMouseInteractor
            List<GameObject> spatialMouseChildren = new List<GameObject>();
            spatialMouseGameObject.GetChildGameObjects(spatialMouseChildren);
            var spatialMouseInteractorGameObject = spatialMouseChildren.Find(go => go.name.Equals(SpatialMouseInteractorName));
            var spatialMouseInteractor = spatialMouseInteractorGameObject.GetComponent<SpatialMouseInteractor>();
            Assert.IsNotNull(spatialMouseInteractor);

            // Check XRI3+ input action configuration is correct
            Assert.IsNotNull(spatialMouseInteractor.mouseMoveAction);
            Assert.IsNotNull(spatialMouseInteractor.mouseScrollAction);
            Assert.IsNotNull(spatialMouseInteractor.mouseMoveAction.reference);
            Assert.IsNotNull(spatialMouseInteractor.mouseScrollAction.reference);
            Assert.IsTrue(spatialMouseInteractor.mouseMoveAction.reference.action.name.Equals(MouseMoveName));
            Assert.IsTrue(spatialMouseInteractor.mouseScrollAction.reference.action.name.Equals(MouseScroll));

            yield return null;
        }

        /// <summary>
        /// Creates and returns the Spatial Mouse Controller.
        /// </summary>
        public static GameObject InstantiateSpatialMouseController()
        {
            Object prefab = AssetDatabase.LoadAssetAtPath(SpatialMouseControllerPrefabPath, typeof(Object));
            spatialMouseGameObject = Object.Instantiate(prefab) as GameObject;
            return spatialMouseGameObject;
        }
    }
}
#pragma warning restore CS1591
