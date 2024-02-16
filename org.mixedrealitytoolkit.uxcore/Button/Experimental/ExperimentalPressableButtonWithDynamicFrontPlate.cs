// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.UI;

namespace MixedReality.Toolkit.UX
{
    /// <summary>
    /// ToDo : Complete this summary
    /// </summary>
    /// <remarks>
    /// ToDo : Complete this remarks
    /// </remarks>
    [RequireComponent(typeof(PressableButton))]
    public class ExperimentalPressableButtonWithDynamicFrontPlate : MonoBehaviour
    {
        /// <summary>
        /// ToDo : Complete this summary
        /// </summary>
        private const string FrontPlateName = "Frontplate";

        /// <summary>
        /// Stores the FrontPlate's RawImage component if this is an EmptyButton, ActionButton, or CanvasButtonToggleSwitch.  Null otherwise.
        /// Populated during runtime on this MonoBehaviour Start method.
        /// </summary>
        private RawImage frontPlateRawImage = null;

        #region Private Members

        /// <summary>
        /// Gets the front plate's raw image component if this is an EmptyButton (Experimental), ActionButton (Experimental), or CanvasButtonToggleSwitch (Experimental).  Null otherwise.
        /// </summary>
        /// <returns>Reference to this button FrontPlate's RawImage Component.  Null if it doesn't exist or if this is not an EmptyButton (Experimental), ActionButton (Experimental), or CanvasButtonToggleSwitch (Experimental)</returns>
        internal RawImage GetFrontPlateRawImage()
        {
            if (gameObject.tag.Equals("ExperimentalDynamicFrontplate")) //This is a temporary conditional for the experimental dynamic frontplate feature, it will be removed if and only if the community accepts the experimental feature to be included in official release and be integrated as part of PressableButton script
            {
                foreach (Transform child in transform)
                {
                    if (child.name.Equals(FrontPlateName))
                    {
                        return child.GetComponent<RawImage>();
                    }
                }
            }
            return null;
        }

        #endregion Private Members

        /// <summary>
        /// A Unity event function that is called on the frame when a script is enabled just before any of the update methods are called the first time.
        /// </summary> 
        private void Start()
        {
            frontPlateRawImage = GetFrontPlateRawImage();
        }

        /// <summary>
        /// ToDo: Complete this summary
        /// </summary>
        public void OnProximityHoverEntered()
        {
            //Debug.Log(name + " >> OnProximityHoverENTERED"); //ToDo : Remove this for final PR, it is currently used for debugging during development
            if (frontPlateRawImage != null)
            {
                frontPlateRawImage.enabled = true;
                //Debug.Log(name + " Frontplate RawImage ENABLED"); //ToDo : Remove this for final PR, it is currently used for debugging during development
            }
        }

        /// <summary>
        /// ToDo: Complete this summary
        /// </summary>
        public void OnProximityHoverExited()
        {
            //Debug.Log(name + " >> OnProximityHoverEXITED"); //ToDo : Remove this for final PR, it is currently used for debugging during development
            if (frontPlateRawImage != null)
            {
                frontPlateRawImage.enabled = false;
                //Debug.Log(name + " Frontplate RawImage DISABLED"); //ToDo : Remove this for final PR, it is currently used for debugging during development
            }
        }
    }
}
