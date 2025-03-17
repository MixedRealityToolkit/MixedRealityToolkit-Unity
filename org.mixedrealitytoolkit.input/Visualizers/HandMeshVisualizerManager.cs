// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// Manages the visibility of a set of hand mesh visualizers, all representing possible
    /// visualization modes for a single hand (like platform-provided vs the built-in rigged hand mesh).
    /// </summary>
    public class HandMeshVisualizerManager : MonoBehaviour
    {
        [SerializeField, Tooltip("A priority-ordered visualizer list.")]
        private HandMeshVisualizer[] visualizers;

        protected void Update()
        {
            int renderingVisualizer = -1;

            for (int i = 0; i < visualizers.Length; i++)
            {
                // If we've already found a rendering visualizer, we want to turn off the rest
                if (renderingVisualizer > -1)
                {
                    visualizers[i].enabled = false;
                }
                else if (visualizers[i].IsRendering)
                {
                    renderingVisualizer = i;
                }
            }

            // Ensure any visualizers that preceded the rendering one are turned off
            for (int i = 0; i < renderingVisualizer; i++)
            {
                visualizers[i].enabled = false;
            }

            if (renderingVisualizer == -1)
            {
                for (int i = 0; i < visualizers.Length; i++)
                {
                    visualizers[i].enabled = true;
                }
            }
        }
    }
}
