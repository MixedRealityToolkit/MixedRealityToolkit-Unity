// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using MixedReality.Toolkit.Core.Tests;
using System;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Interactions;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Composites;
using UnityEngine;
using static MixedReality.Toolkit.Input.Tests.InputTestUtilities;

namespace MixedReality.Toolkit.Input.Tests
{
    /// <summary>
    /// Abstract base class for all automated tests that require input simulation.
    /// The included setup and teardown methods will setup/teardown the MRTK rig,
    /// as well as the simulated input devices.
    /// </summary>
    public abstract class BaseRuntimeInputTests : BaseRuntimeTests
    {

        // Isolates/sandboxes the input system state for each test instance.
        private InputTestFixture input = new InputTestFixture();

        private XRInteractionManager cachedInteractionManager = null;

        /// <summary>
        /// A cached reference to the <see cref="XRInteractionManager"/> on the XR rig.
        /// Cleared during <see cref="TearDown"/> at the end of each test.
        /// </summary>
        protected XRInteractionManager CachedInteractionManager
        {
            get
            {
                if (cachedInteractionManager == null)
                {
                    cachedInteractionManager = FindObjectUtility.FindAnyObjectByType<XRInteractionManager>();
                }
                return cachedInteractionManager;
            }
        }

        private TrackedPoseDriverLookup cachedTrackedPoseDriverLookup;

        /// <summary>
        /// A cached reference to the <see cref="TrackedPoseDriverLookup"/> on the XRI3+ rig.
        /// Cleared during <see cref="TearDown"/> at the end of each test.
        /// </summary>
        protected TrackedPoseDriverLookup CachedTrackedPoseDriverLookup
        {
            get
            {
                if (cachedTrackedPoseDriverLookup == null && CachedInteractionManager == null)
                {
                    Debug.LogError("Unable to get a reference to Rig's TrackedPoseDriverLookup because CachedInteractionManager is null.");
                    return null;
                }
                cachedTrackedPoseDriverLookup = CachedInteractionManager.gameObject.GetComponent<TrackedPoseDriverLookup>();

                return cachedTrackedPoseDriverLookup;
            }
        }

        /// <summary>
        /// Get the version of the input rig to use with these tests
        /// </summary>
        protected virtual RigVersion RigVersion { get; } = RigVersion.Default;

#pragma warning disable CS0618 // Type or member is obsolete
        private ControllerLookup cachedLookup = null;

        /// <summary>
        /// A cached reference to the <see cref="ControllerLookup"/> on the XR rig.
        /// Cleared during <see cref="TearDown"/> at the end of each test.
        /// </summary>
        protected ControllerLookup CachedLookup
        {
            get
            {
                if (cachedLookup == null)
                {
                    if (CachedInteractionManager == null)
                    {
                        return null;
                    }
                    cachedLookup = CachedInteractionManager.gameObject.GetComponent<ControllerLookup>();
                }
                return cachedLookup;
            }
        }
#pragma warning restore CS0618 // Type or member is obsolete

        public override IEnumerator Setup()
        {
            yield return base.Setup();
            InputSystemSetup();
            XRISetup();


            InputTestUtilities.InstantiateRig(RigVersion);
            InputTestUtilities.SetupSimulation(0.0f);

            // Wait for simulation HMD to update camera poses
            yield return RuntimeTestUtilities.WaitForUpdates();
        }

        private void InputSystemSetup()
        {
            input.Setup();
            InputSystem.onAfterUpdate += OnAfterUpdate;
        }

        private void XRISetup()
        {
            // XRI needs these
            InputSystem.RegisterInteraction<SectorInteraction>();
            InputSystem.RegisterBindingComposite<Vector3FallbackComposite>();
            InputSystem.RegisterBindingComposite<QuaternionFallbackComposite>();
            InputSystem.RegisterBindingComposite<IntegerFallbackComposite>();
        }

        public override IEnumerator TearDown()
        {
            yield return null; // Make sure the input system gets one last tick.
            InputTestUtilities.TeardownRig();
            InputTestUtilities.TeardownSimulation();
            cachedInteractionManager = null;
            cachedLookup = null;
            cachedTrackedPoseDriverLookup = null;

            input.TearDown();
            InputSystem.onAfterUpdate -= OnAfterUpdate;

            yield return base.TearDown();
        }

        /// <summary>
        /// Update the simulation immediately after the input system has updated. For more details see <see cref="InputTestUtilities.HandUpdate"/>.
        /// </summary>
        private void OnAfterUpdate()
        {
            InputTestUtilities.UpdateSimulation();
        }
    }
}
#pragma warning restore CS1591
