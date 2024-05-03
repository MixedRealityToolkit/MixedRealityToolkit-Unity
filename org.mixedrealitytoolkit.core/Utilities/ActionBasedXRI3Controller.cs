using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace MixedReality.Toolkit
{
    public class ActionBasedXRI3Controller : MonoBehaviour
    {
        // Parent Controller TrackedPoseDriver
        [SerializeField]
        [Tooltip("The parent TrackedPoseDriver component, can be a different though.")]
        private TrackedPoseDriver parentControllerTrackedPoseDriver = null;

        public TrackedPoseDriver ParentControllerTrackedPoseDriver //ToDo: May not be needed, check if it is used
        {
            get => parentControllerTrackedPoseDriver;
            set => parentControllerTrackedPoseDriver = value;
        }

        #region Code from XRBaseController previous to XRI 3 migration

        XRControllerState m_ControllerState;
        /// <summary>
        /// The current state of the controller.
        /// </summary>
        public XRControllerState currentControllerState
        {
            get
            {
                //SetupControllerState(); //ToDo: implement this
                return m_ControllerState;
            }

            set
            {
                m_ControllerState = value;
                m_CreateControllerState = false;
            }
        }

        bool m_CreateControllerState = true;

        #endregion Code from XRBaseController previous to XRI 3 migration

        // Start is called before the first frame update
        void Start()
        {
            if (ParentControllerTrackedPoseDriver == null)
            {
                Debug.LogWarning($"This ActionBasedXRI3Controller is missing its TrackedPoseDriver, it should probably be the one from '{transform.parent.name}' but can be another one.");
            }
        }

    }
}
