// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// This component instantiates a model prefab for hand interactors.
    /// </summary>
    /// <remarks>
    /// This does not control the visibility of the instantiated models, the prefab is always created.
    /// </remarks>
    public class HandModel : MonoBehaviour
    {
        #region Properties

        [SerializeField, Tooltip("The prefab of the MRTK Controller to show that will be automatically instantiated by this behavior.")]
        private Transform modelPrefab;

        /// <summary>
        /// The prefab of the model to show that will be automatically instantiated by this <see cref="MonoBehaviour"/>.
        /// </summary>
        public Transform ModelPrefab
        {
            get => modelPrefab;
            set => modelPrefab = value;
        }

        [SerializeField, Tooltip("The transform that is used as the parent for the model prefab when it is instantiated. Will be set to a new child GameObject if None.")]
        private Transform modelParent;

        /// <summary>
        /// The <see cref="Transform"/> that is used as the parent for the model prefab when it is instantiated. Will be set to a new child <see cref="GameObject"/> if None.
        /// </summary>
        public Transform ModelParent => modelParent;

        [SerializeField, Tooltip("The instance of the controller model in the scene. This can be set to an existing object instead of using Model Prefab.")]
        private Transform model;

        /// <summary>
        /// The instance of the model in the scene. This can be set to an existing object instead of using Model Prefab.
        /// </summary>
        public Transform Model => model;

        #endregion Properties

        #region Associated hand select values

        [SerializeField, Tooltip("The XRInputButtonReader representing selection values to be used by the hand model prefab when implementing ISelectInputVisualizer.")]
        private XRInputButtonReader selectInput;

        /// <summary>
        /// The <see cref="XRInputButtonReader"/> representing selection values to be used by
        /// the hand model prefab when implementing <see cref="ISelectInputVisualizer"/>.
        /// </summary>
        public XRInputButtonReader SelectInput => selectInput;
        
        #endregion Associated hand select values

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void Awake()
        {
            // Create empty container transform for the model if none specified.
            // This is not strictly necessary to create since this GameObject could be used
            // as the parent for the instantiated prefab, but doing so anyway for backwards compatibility.
            if (modelParent == null)
            {
                modelParent = new GameObject($"[{gameObject.name}] Model Parent").transform;
                modelParent.SetParent(transform, false);
                modelParent.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
        }

        /// <summary>
        /// A Unity event function that is called on the frame when a script is enabled just before any of the update methods are called the first time.
        /// </summary>
        protected virtual void Start()
        {
            // Instantiate the model prefab if it is set
            if (modelPrefab != null)
            {
                model = Instantiate(modelPrefab, modelParent);

                Debug.Assert(selectInput != null, $"The Select Input reader for {name} is not set and will not be used with the instantiated hand model.");

                // Set the select input reader for the model if it implements ISelectInputVisualizer
                if (selectInput != null && model != null && model.TryGetComponent(out ISelectInputVisualizer selectInputVisualizer))
                {
                    selectInputVisualizer.SelectInput = selectInput;
                }
            }

            MRTKInputFocusManager.OnXrSessionFocus.AddListener(OnXrSessionFocus);
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        private void OnDestroy() => MRTKInputFocusManager.OnXrSessionFocus.RemoveListener(OnXrSessionFocus);

        /// <summary>
        /// Sent to all GameObjects when the player gets or loses focus.
        /// </summary>
        /// <param name="focus"><see langword="true"/> if the GameObjects have focus, else <see langword="false"/>.</param>
        private void OnXrSessionFocus(bool focus)
        {
            // We want to ensure we're focused for input visualization, as some runtimes continue reporting "tracked" while pose updates are paused.
            // This is allowed, per-spec, as a "should": "Runtimes should make input actions inactive while the application is unfocused,
            // and applications should react to an inactive input action by skipping rendering of that action's input avatar
            // (depictions of hands or other tracked objects controlled by the user)."

            if (modelParent != null)
            {
                modelParent.gameObject.SetActive(focus);
            }
        }
    }
}
