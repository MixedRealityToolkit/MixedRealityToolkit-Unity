// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MixedReality.Toolkit
{
    /// <summary>
    /// Extension methods that make working with the <see cref="InteractorHandedness"/> enum easier.
    /// </summary>
    public static class InteractorHandednessExtensions
    {
        /// <summary>
        /// Gets the <see cref="XRNode"/> representing the specified <see cref="InteractorHandedness"/>. If the <see cref="InteractorHandedness"/>
        /// is other than InteractorHandedness.Left or InteractorHandedness.Right then it defaults to XRNode defaultValue parameter.
        /// </summary>
        /// <param name="hand">The <see cref="InteractorHandedness"/> value for
        /// which the <see cref="XRNode"/> is requested.</param>
        /// <param name="defaultValue">The default <see cref="XRNode"/> value to return if the <see cref="InteractorHandedness"/> is neither
        /// InteractorHandedness.Left nor InteractorHandedness.Right.</param>
        /// <returns>
        /// <see cref="XRNode"/> representing the specified <see cref="InteractorHandedness"/>.
        /// </returns>

        /// <summary>
        /// Gets the <see cref="XRNode"/> representing the specified <see cref="InteractorHandedness"/>. If the <see cref="InteractorHandedness"/>
        /// </summary>
        public static XRNode ToXRNode(this InteractorHandedness hand, XRNode defaultValue = XRNode.RightHand)
        {
            switch (hand)
            {
                case InteractorHandedness.Left:
                    return XRNode.LeftHand;
                case InteractorHandedness.Right:
                    return XRNode.RightHand;
                default:
                    return defaultValue;
            }
        }
    }
}
