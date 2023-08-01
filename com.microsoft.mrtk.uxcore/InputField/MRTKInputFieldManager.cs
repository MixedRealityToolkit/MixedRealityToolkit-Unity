// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine.UI;

namespace MixedReality.Toolkit.UX
{
    /// <summary>
    /// Static class recording the currently active input field in the scene.
    /// </summary>
    /// <remarks>
    /// <para>The class is mainly used to workaround limitations within the EventSystem/InputModule scripts that
    /// adversely impact the lifecycle management of input fields.</para>
    /// </remarks>
    public static class MRTKInputFieldManager
    {
        private static Selectable currentInputField = null;

        /// <summary>
        /// Set the provided input field to be the currently active input field in the scene.
        /// </summary>
        public static void SetCurrentInputField(Selectable newInputField)
        {
            if (currentInputField != null)
            {
                currentInputField.OnDeselect(null);
            }
            currentInputField = newInputField;
        }

        /// <summary>
        /// Clear the recorded currently active input field if the provided input field is a match to the recorded one.
        /// </summary>
        public static void RemoveCurrentInputField(Selectable inputField)
        {
            if (currentInputField == inputField && inputField != null)
            {
                currentInputField = null;
            }
        }
    }
}
