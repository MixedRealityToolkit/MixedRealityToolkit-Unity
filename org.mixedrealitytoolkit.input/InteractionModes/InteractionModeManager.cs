// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

using TrackedPoseDriver = UnityEngine.InputSystem.XR.TrackedPoseDriver;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// Used to manage interactors and ensure that each several interactors for an interactor group aren't clashing and firing at the same time.
    /// </summary>
    [AddComponentMenu("MRTK/Input/Interaction Mode Manager")]
    public class InteractionModeManager : MonoBehaviour
    {
        /// <summary>
        /// Describes the types of interaction modes an interactor can belong to
        /// </summary>
        [Serializable]
        private class ManagedInteractorStatus
        {
            [SerializeField]
            [Tooltip("A value representing interactor mode or state that is being targeted by this Managed Interactor Status.")]
            private InteractionMode currentMode;

            /// <summary>
            /// Get or set the value representing interactor mode or state that is being targeted by the <see cref="ManagedInteractorStatus"/> instance.
            /// </summary>
            public InteractionMode CurrentMode
            {
                get => currentMode;
                set => currentMode = value;
            }

            [SerializeField]
            [Tooltip("The interactor mode or state that is being targeted by this Managed Interactor Status.")]
            private List<XRBaseInteractor> interactors = new List<XRBaseInteractor>();

            /// <summary>
            /// The interactor mode or state that is being targeted by the <see cref="ManagedInteractorStatus"/> instance.
            /// </summary>
            public List<XRBaseInteractor> Interactors => interactors;
        }

#if UNITY_EDITOR
        private static InteractionModeManager activeInstance;
        /// <summary>
        /// The current active instance of the Interaction Mode Manager
        /// Only one interaction mode manager should be present in a scene at any given time
        /// </summary>
        public static InteractionModeManager Instance
        {
            get
            {
                if (activeInstance == null)
                {
                    activeInstance = UnityEditor.SceneManagement.StageUtility.GetCurrentStageHandle().FindComponentOfType<InteractionModeManager>();
                }

                return activeInstance;
            }
        }

        /// <summary>
        /// Editor only function for initializing the Interaction Mode Manager with the existing XR controllers in the scene
        /// </summary>
        [Obsolete("This method is obsolete. Please use InitializeInteractorGroups instead.")]
        public void InitializeControllers()
        {
            foreach (XRController xrController in FindObjectUtility.FindObjectsByType<XRController>())
            {
                if (!interactorGroupMappings.ContainsKey(xrController.gameObject))
                {
                    interactorGroupMappings.Add(xrController.gameObject, new ManagedInteractorStatus());
                }
            }
        }

        /// <summary>
        /// Editor only function for initializing the Interaction Mode Manager with the existing interactors in the scene.
        /// </summary>
        /// <remarks>
        /// This will group interactors according to the game object returned by <see cref="IModeManagedInteractor"/>. If the interactor does
        /// not implement <see cref="IModeManagedInteractor"/> or if  <see cref="IModeManagedInteractor.ModeManagedRoot"/> is null the interactor
        /// will not automatically be tracked by this component.
        /// </remarks>
        public void InitializeInteractorGroups()
        {
            interactorGroupMappings.Clear();

            foreach (XRBaseInteractor xrInteractor in FindObjectUtility.FindObjectsByType<XRBaseInteractor>())
            {
                if (xrInteractor is IModeManagedInteractor modeManagedInteractor &&
                    modeManagedInteractor.ModeManagedRoot != null)
                {
                    interactorGroupMappings.TryAdd(modeManagedInteractor.ModeManagedRoot, new ManagedInteractorStatus());
                }
            }

            // For backwards compatibility, we will continue to support the obsolete "controller" types, and group based on "controller" parents.
            // Once XRI removes "controller" types, we can remove this block of code.
#pragma warning disable CS0618 // InitializeControllers is obsolete
            InitializeControllers();
#pragma warning restore CS0618 // InitializeControllers is obsolete
        }

        /// <summary>
        /// Expands this object's <see cref="PrioritizedInteractionModes"/> property with base and sub types associated with
        /// the current value stored in the <see cref="PrioritizedInteractionModes"/> property.
        /// </summary>
        /// <remarks>
        /// This function is only intended for use in Unity's inspector window. See 
        /// `InteractionModeManagerEditor` documentation for more details.
        /// </remarks>
        public void PopulateModesWithSubtypes()
        {
            List<InteractionModeDefinition> newPrioritizedInteractionModes = new List<InteractionModeDefinition>();

            for (int i = 0; i < prioritizedInteractionModes.Count; i++)
            {
                InteractionModeDefinition mode = prioritizedInteractionModes[i];

                List<SystemType> subtypes = new List<SystemType>();
                foreach (SystemType baseType in mode.AssociatedTypes)
                {
                    subtypes.Add(baseType);
                    var allSubtypes = AppDomain.CurrentDomain.GetAssemblies()
                         .SelectMany(assembly => assembly.GetLoadableTypes())
                         .Where(type => type.IsSubclassOf(baseType))
                         .ToList();

                    foreach (SystemType type in allSubtypes)
                    {
                        subtypes.Add(type);
                    }
                }

                newPrioritizedInteractionModes.Add(new InteractionModeDefinition(mode.ModeName, subtypes));
            }
            prioritizedInteractionModes = newPrioritizedInteractionModes;
        }
#endif

        /// <summary>
        /// Initializes the Interaction Mode Manager with the existing Interaction Mode Detectors in the scene
        /// </summary>
        public void InitializeInteractionModeDetectors()
        {
            interactionModeDectectors.Clear();

            // PERFORMANCE FIXME: This is not great for performance. Find better way to register detectors?
            // We would query interactors and then add all interactors that happen to be a detector, but
            // detectors may not necessarily be interactors.
            foreach (IInteractionModeDetector detector in FindObjectUtility.FindObjectsByType<MonoBehaviour>().OfType<IInteractionModeDetector>())
            {
                interactionModeDectectors.Add(detector);
            }
        }

        /// <summary>
        /// The list of interaction mode detectors.
        /// </summary>
        private List<IInteractionModeDetector> interactionModeDectectors = new List<IInteractionModeDetector>();

        /// <summary>
        /// Mapping of the root game objects to the set of interactors that will be managed as a group.
        /// </summary>
        /// <remarks>
        /// The MRTK Interaction Mode Manager will only mediate interactors which are designated as managed.
        /// </remarks>
        [SerializeField]
        [FormerlySerializedAs("controllerMapping")]
        [Tooltip("Mapping of the root game objects to the set of interactors that will be managed as a group. The MRTK Interaction Mode Manager will only mediate interactors which are designated as managed")]
        private SerializableDictionary<GameObject, ManagedInteractorStatus> interactorGroupMappings = new SerializableDictionary<GameObject, ManagedInteractorStatus>();

        /// <summary>
        /// Private collection kept in lock-step with interactorMapping. Used to keep track of all registered interactors.
        /// Interactors are only registered once, when they are created. They are also unregistered once, when their reference becomes null.
        /// </summary>
        private HashSet<XRBaseInteractor> registeredInteractors = new HashSet<XRBaseInteractor>();

        [SerializeField]
        [Tooltip("Describes the order of priority that interactor types have over each other.")]
        private List<InteractionModeDefinition> prioritizedInteractionModes = new List<InteractionModeDefinition>();

        /// <summary>
        /// Describes the order of priority that interactor types have over each other.
        /// </summary>
        public List<InteractionModeDefinition> PrioritizedInteractionModes => prioritizedInteractionModes;

        [SerializeField]
        [Tooltip("The default interaction mode when no other mode has been specified.")]
        private InteractionMode defaultMode;

        /// <summary>
        /// The default interaction mode when no other mode has been specified.
        /// </summary>
        public InteractionMode DefaultMode
        {
            get => defaultMode;
            set => defaultMode = value;
        }

        #region Internal protected properties

        private XRInteractionManager interactionManager;

        /// <summary>
        /// The interaction manager to use to query interactors and their registration events.
        /// Currently protected internal, may be exposed in a future update.
        /// </summary>
        internal protected XRInteractionManager InteractionManager
        {
            get
            {
                if (interactionManager == null)
                {
                    interactionManager = ComponentCache<XRInteractionManager>.FindFirstActiveInstance();
                }

                return interactionManager;
            }
            set => interactionManager = value;
        }

        #endregion Internal protected properties

        /// <summary>
        /// Registers an interactor to be managed by the interaction mode manager
        /// </summary>
        /// <param name="interactor">An XRBaseInteractor which needs to be managed based on interaction modes</param>
        public void RegisterInteractor(XRBaseInteractor interactor)
        {
            // Only register interactor groups which are governed by some kind of interaction mode
            if (!IsInteractorValid(interactor))
            {
                return;
            }

            GameObject interactorGroupObject = FindInteractorGroupObject(interactor);

            Assert.IsNotNull(interactorGroupObject, $"Interactor {interactor.name} ({interactor.GetType().Name}) is not managed by any interactor group. " + Environment.NewLine +
                    $"Please ensure that the interactor implements IModeManagedInteractor, has a ModeManagedRoot field, and that ModeManagedRoot is set to the parent GameObject.");

            if (!interactorGroupMappings.ContainsKey(interactorGroupObject))
            {
                interactorGroupMappings.Add(interactorGroupObject, new ManagedInteractorStatus());
            }

            if (!registeredInteractors.Contains(interactor))
            {
                interactorGroupMappings[interactorGroupObject].Interactors.Add(interactor);
                registeredInteractors.Add(interactor);
            }
        }

        /// <summary>
        /// This unregisters an interactor from this <see cref="InteractionModeManager"/>.
        /// </summary>
        /// <remarks>
        /// This is used when the interactor's game object is destroyed or when 
        /// it is no longer meant to be used in the scene.
        /// 
        /// This function should not be called by the <see cref="InteractionManager"/> object. If it were, this class
        /// would receive an unregister event every time an interactor was disabled. This function should  
        /// only be called when an interactor is removed scene completely; for example, 
        /// when a interactor group's game object is destroyed.
        /// </remarks>
        /// <param name="interactor">The <see cref="XRBaseInteractor"/> to be unregistered.</param>
        public void UnregisterInteractor(XRBaseInteractor interactor)
        {
            GameObject interactorGroupObject = FindInteractorGroupObject(interactor);

            if (interactorGroupMappings.TryGetValue(interactorGroupObject, out ManagedInteractorStatus managedInteractorStatus))
            {
                managedInteractorStatus.Interactors.Remove(interactor);
            }
            registeredInteractors.Remove(interactor);
        }

        /// <summary>
        /// A Unity event function that is called when an enabled script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            // Sanity check making sure that there are no duplicate entries in the prioritized interaction mode list
            if (InteractionManager != null)
            {
                // We only listen to interactor registrations, not deregistrations,
                // because we are going to be in charge of deregistering interactors when
                // their mode is not active. We manually call our own deregistration function
                // when an interactor will be permanently removed from play, such as when
                // the interactor group's game object is destroyed.
                InteractionManager.interactorRegistered += OnInteractorRegistered;

                List<IXRInteractor> interactors = new List<IXRInteractor>();
                InteractionManager.GetRegisteredInteractors(interactors);

                // Fire a registration event for all pre-existing interactors.
                foreach (IXRInteractor interactor in interactors)
                {
                    if (interactor is XRBaseInteractor baseInteractor)
                    {
                        RegisterInteractor(baseInteractor);
                    }
                }
            }

            // Validate that the list of Interaction Modes is valid
            OnValidate();

            // Go find all detectors.
            InitializeInteractionModeDetectors();
        }

        /// <summary>
        /// A Unity Editor only event function that is called when the script is loaded or a value changes in the Unity Inspector.
        /// </summary>
        private void OnValidate()
        {
            ValidateInteractionModes();
            HashSet<string> duplicatedNames = GetDuplicateInteractionModes();
            if (duplicatedNames.Count > 0)
            {
                var duplicatedNameString = CompileDuplicatedNames(duplicatedNames);

                Debug.LogError($"Duplicate interaction mode definitions detected in the interaction mode manager on {gameObject.name}. " +
                                    $"Please check the following interaction modes: {duplicatedNameString}");
            }
        }

        /// <summary>
        /// This internal function ensures that the changes made in editor are reflected in the internal associatedTypes data structure.
        /// This allows for runtime editing of an interaction modes associated types in editor.
        /// </summary>
        internal void ValidateInteractionModes()
        {
            foreach (InteractionModeDefinition mode in PrioritizedInteractionModes)
            {
                mode.InitializeAssociatedTypes();
            }
        }

        internal HashSet<string> GetDuplicateInteractionModes()
        {
            // First check for any duplicated interaction modes
            HashSet<string> seenNames = new HashSet<string>();
            HashSet<string> duplicatedNames = new HashSet<string>();

            foreach (InteractionModeDefinition mode in PrioritizedInteractionModes)
            {
                string name = mode.ModeName;

                if (seenNames.Contains(name))
                {
                    duplicatedNames.Add(name);
                }
                else
                {
                    seenNames.Add(name);
                }
            }

            return duplicatedNames;
        }

        internal string CompileDuplicatedNames(HashSet<string> duplicatedNames)
        {
            string duplicatedNameString = "";
            int i = 0;
            foreach (var duplicatedName in duplicatedNames)
            {
                i++;
                if (i < duplicatedNames.Count)
                {
                    duplicatedNameString += duplicatedName + ", ";
                }
                else
                {
                    duplicatedNameString += duplicatedName;
                }
            }
            return duplicatedNameString;
        }

        /// <summary>
        /// Private callback fired when an interactor is registered with the
        /// <see cref="InteractionManager"/>.
        /// </summary>
        private void OnInteractorRegistered(InteractorRegisteredEventArgs args)
        {
            if (args.interactorObject is XRBaseInteractor interactor)
            {
                RegisterInteractor(interactor);
            }
        }

        /// <summary>
        /// Caches interactors which have been destroyed but not yet unregistered from the interactor mediator
        /// </summary>
        private List<XRBaseInteractor> destroyedInteractors = new List<XRBaseInteractor>();

        /// <summary>
        /// Caches interactor groups which have been destroyed but not yet unregistered from the interactor mediator
        /// </summary>
        private List<GameObject> destroyedGroups = new List<GameObject>();

        /// <summary>
        /// Marks interactor groups that have been modified by a detector, so other detectors
        /// don't overwrite their changes.
        /// </summary>
        private HashSet<GameObject> modifiedGroupsThisFrame = new HashSet<GameObject>();

        private static readonly ProfilerMarker UpdatePerfMarker =
            new ProfilerMarker("[MRTK] InteractionModeManager.Update");

        /// <summary>
        /// A Unity event function that is called every frame, if this object is enabled.
        /// </summary>
        private void Update()
        {
            using (UpdatePerfMarker.Auto())
            {
                modifiedGroupsThisFrame.Clear();

                // Updating the status of all interactor groups based on their interaction mode
                foreach (IInteractionModeDetector detector in interactionModeDectectors)
                {
                    List<GameObject> groups = detector.GetInteractorGroups();

                    // For backwards compatibility, we will continue to support the obsolete "GetControllers()" function.
                    if (groups == null)
                    {
#pragma warning disable CS0618 // GetControllers is obsolete
                        groups = detector.GetControllers();
#pragma warning restore CS0618 // GetControllers is obsolete
                    }

                    foreach (GameObject group in groups)
                    {
                        if (detector.IsModeDetected())
                        {
                            SetInteractionMode(group, detector.ModeOnDetection);

                            // Mark this group as modified this frame.
                            modifiedGroupsThisFrame.Add(group);
                        }
                        // Reset mode, if and only if none of the other detectors
                        // have not modified it this frame.
                        else if (!modifiedGroupsThisFrame.Contains(group))
                        {
                            ResetToDefaultMode(group);
                        }
                    }
                }

                destroyedGroups.Clear();
                destroyedInteractors.Clear();

                foreach (GameObject groupObject in interactorGroupMappings.Keys)
                {
                    // If the group object has be destroyed, be sure to mark it and its interactors for unregistration
                    if (groupObject == null)
                    {
                        destroyedGroups.Add(groupObject);
                        foreach (XRBaseInteractor interactor in interactorGroupMappings[groupObject].Interactors)
                        {
                            destroyedInteractors.Add(interactor);
                        }
                        continue;
                    }

                    // mediating all of the interactors to ensure the correct ones are active for their interactor group's given interaction mode
                    InteractionModeDefinition groupCurrentMode = prioritizedInteractionModes[interactorGroupMappings[groupObject].CurrentMode.Priority];

                    foreach (XRBaseInteractor interactor in interactorGroupMappings[groupObject].Interactors)
                    {
                        // If the interactor has be destroyed, be sure to mark it for unregistration
                        if (interactor == null)
                        {
                            destroyedInteractors.Add(interactor);
                            continue;
                        }

                        interactor.enabled = IsInteractorValidForMode(groupCurrentMode, interactor);
                    }
                }

                foreach (GameObject groupObject in destroyedGroups)
                {
                    destroyedGroups.Remove(groupObject);
                }

                foreach (XRBaseInteractor interactor in destroyedInteractors)
                {
                    UnregisterInteractor(interactor);
                }
            }
        }

        /// <summary>
        /// Sets the interaction mode for the target group object.
        /// </summary>
        /// <param name="groupObject">The group object we need to toggle the mode of</param>
        /// <param name="interactionMode">The interaction mode that is currently being applied to this interactor group.</param>
        public void SetInteractionMode(GameObject groupObject, InteractionMode interactionMode)
        {
            if (interactorGroupMappings.TryGetValue(groupObject, out ManagedInteractorStatus managedInteractorStatus))
            {
                managedInteractorStatus.CurrentMode = managedInteractorStatus.CurrentMode.Priority > interactionMode.Priority ? managedInteractorStatus.CurrentMode : interactionMode;
            }
        }

        /// <summary>
        /// Resets the group's interaction mode to the default mode specified on the interaction mode manager
        /// </summary>
        /// <param name="groupObject">The group we intend to reset to the default mode</param>
        public void ResetToDefaultMode(GameObject groupObject)
        {
            if (interactorGroupMappings.TryGetValue(groupObject, out ManagedInteractorStatus managedInteractorStatus))
            {
                managedInteractorStatus.CurrentMode = defaultMode;
            }
        }

        private bool IsInteractorValidForMode(InteractionModeDefinition mode, XRBaseInteractor interactor)
        {
            return mode.AssociatedTypes.Contains(interactor.GetType());
        }

        /// <summary>
        /// Maps an interactor's system.type to a InteractorType enum, describing various modes of interaction.
        /// </summary>
        /// <param name="interactor">The interactor we wish to check</param>
        /// <returns>Returns whether or not the interactor is governed by any of the defined interaction modes</returns>
        private bool IsInteractorValid(XRBaseInteractor interactor)
        {
            for (int i = 0; i < prioritizedInteractionModes.Count; i++)
            {
                if (IsInteractorValidForMode(prioritizedInteractionModes[i], interactor))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Query the interactor for the interactor group that it should be managed under.
        /// </summary>
        private GameObject FindInteractorGroupObject(XRBaseInteractor interactor)
        {
            GameObject interactorGroupObject = null;

            // For backwards compatibility, we will continue to support the obsolete "controller-based" interactors,
            // and group based on "controller" partents.
#pragma warning disable CS0618 // xrController is obsolete
            if (interactor is XRBaseInputInteractor controllerInteractor &&
                controllerInteractor.xrController != null)
            {
                interactorGroupObject = controllerInteractor.xrController.gameObject;
            }
#pragma warning restore CS0618 // xrController is obsolete
            else if (interactor is IModeManagedInteractor modeManagedInteractor)
            {
                interactorGroupObject = modeManagedInteractor.ModeManagedRoot;

                // For backwards compatibility, we will continue to support the obsolete "GetModeManagedController()" function.
                if (interactorGroupObject == null)
                {
#pragma warning disable CS0618 // GetModeManagedController is obsolete
                    interactorGroupObject = modeManagedInteractor.GetModeManagedController();
#pragma warning restore CS0618 // GetModeManagedController is obsolete
                }
            }

            return interactorGroupObject;
        }
    }
}
