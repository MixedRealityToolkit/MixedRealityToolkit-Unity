// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// Defines an interface for providing an input reader for selection values
    /// in order to be incorporated by a visualizer.
    /// </summary>
    public interface ISelectInputVisualizer
    {
        /// <summary>
        /// Input reader for select input to be used in visualization.
        /// </summary>
        public XRInputButtonReader SelectInput
        {
            get;
            set;
        }
    }
}
