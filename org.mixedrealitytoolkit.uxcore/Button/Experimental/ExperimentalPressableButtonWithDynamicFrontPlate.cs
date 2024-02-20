// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.UI;

namespace MixedReality.Toolkit.UX
{
    /// <summary>
    /// This mono behaviour is used to enable or disable the front plate of a button when the button enters or exists proximity-hovering.
    /// </summary>
    /// <remarks>
    /// This is an experimental feature. This class is early in the cycle, it has
    /// been labeled as experimental to indicate that it is still evolving, and
    /// subject to change over time. Parts of the MRTK, such as this class, appear
    /// to have a lot of value even if the details haven't fully been fleshed out.
    /// For these types of features, we want the community to see them and get
    /// value out of them early enough so to provide feedback.
    /// </remarks>
    [RequireComponent(typeof(PressableButton))]
    public class ExperimentalPressableButtonWithDynamicFrontPlate : MonoBehaviour
    {
        /// <summary>
        /// Name of the Frontplate GameObject in the prefab.
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
            if (gameObject.tag.Equals("ExperimentalDynamicFrontplate")) //This is a temporary conditional for the experimental dynamic frontplate feature, it will be removed if the community accepts the experimental feature to be included in official release and be integrated as part of PressableButton script
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
        protected void Start()
        {
            frontPlateRawImage = GetFrontPlateRawImage();
        }

        /// <summary>
        /// Method invoked when the button enters proximity-hovering, set in Editor.
        /// </summary>
        public void OnProximityHoverEntered()
        {
            if (frontPlateRawImage != null)
            {
                frontPlateRawImage.enabled = true;
            }
        }

        /// <summary>
        /// Method invoked when the button exits proximity-hovering, set in Editor.
        /// </summary>
        public void OnProximityHoverExited()
        {
            if (frontPlateRawImage != null)
            {
                frontPlateRawImage.enabled = false;
            }
        }
    }
}
