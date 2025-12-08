// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MixedReality.Toolkit
{
    /// <summary>
    /// Methods which extend the functionality of the Unity XRNode struct.
    /// </summary>
    public static class XRNodeExtensions
    {
        /// <summary>
        /// Returns the <see cref="Handedness"/> of the specified XRNode.
        /// </summary>
        /// <param name="node">The XRNode for which the <see cref="Handedness"/> is requested.</param>
        /// <returns>
        /// <see cref="Handedness"/> value representing the XRNode.
        /// </returns>
        /// <remarks>
        /// This will return <see cref="Handedness.None"/> for XRNode values other than
        /// LeftHand or RightHand.
        /// </remarks>
        public static Handedness ToHandedness(this XRNode node) => node switch
        {
            XRNode.LeftHand => Handedness.Left,
            XRNode.RightHand => Handedness.Right,
            _ => Handedness.None,
        };

        /// <summary>
        /// Returns the <see cref="InteractorHandedness"/> of the specified XRNode.
        /// </summary>
        /// <param name="node">The XRNode for which the <see cref="InteractorHandedness"/> is requested.</param>
        /// <returns>
        /// <see cref="InteractorHandedness"/> value representing the XRNode.
        /// </returns>
        /// <remarks>
        /// This will return <see cref="InteractorHandedness.None"/> for XRNode values other than
        /// LeftHand or RightHand.
        /// </remarks>
        public static InteractorHandedness ToInteractorHandedness(this XRNode node) => node switch
        {
            XRNode.LeftHand => InteractorHandedness.Left,
            XRNode.RightHand => InteractorHandedness.Right,
            _ => InteractorHandedness.None,
        };

        /// <summary>
        /// Determine if the specified XRNode represents a hand.
        /// </summary>
        /// <param name="node">The XRNode to be queried.</param>
        /// <returns>
        /// <see langword="true"/> if the specified XRNode represents the left or right hand, or <see langword="false"/>.
        /// </returns>
        public static bool IsHand(this XRNode node)
        {
            return (node.IsLeftHand() || node.IsRightHand());
        }

        /// <summary>
        /// Determine if the specified XRNode represents the left hand.
        /// </summary>
        /// <param name="node">The XRNode to be queried.</param>
        /// <returns>
        /// <see langword="true"/> if the specified XRNode represents the left hand, or <see langword="false"/>.
        /// </returns>
        public static bool IsLeftHand(this XRNode node)
        {
            return (node == XRNode.LeftHand);
        }

        /// <summary>
        /// Determine if the specified XRNode represents the right hand.
        /// </summary>
        /// <param name="node">The XRNode to be queried.</param>
        /// <returns>
        /// <see langword="true"/> if the specified XRNode represents the right hand, or <see langword="false"/>.
        /// </returns>
        public static bool IsRightHand(this XRNode node)
        {
            return (node == XRNode.RightHand);
        }
    }
}
