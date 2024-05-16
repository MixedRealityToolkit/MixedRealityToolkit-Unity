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
        /// is other than InteractorHandedness.Left or InteractorHandedness.Right then it defaults to InteractorHandedness.Right.
        /// </summary>
        /// <param name="hand">The <see cref="InteractorHandedness"/> value for
        /// which the <see cref="XRNode"/> is requested.</param>
        /// <returns>
        /// <see cref="XRNode"/> representing the specified <see cref="InteractorHandedness"/> with InteractorHandedness.Right as default.
        /// </returns>
        public static XRNode ToXRNodeWithRightHandDefault(this InteractorHandedness hand)
        {
            switch (hand)
            {
                case InteractorHandedness.Left:
                    return XRNode.LeftHand;
                case InteractorHandedness.Right:
                default:
                    return XRNode.RightHand;
            }
        }
    }
}
