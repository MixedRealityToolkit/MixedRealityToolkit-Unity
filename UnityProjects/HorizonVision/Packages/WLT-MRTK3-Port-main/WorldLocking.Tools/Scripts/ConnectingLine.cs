// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Tools
{
    /// <summary>
    /// Helper class for visualizing a graph of connected transforms.
    /// </summary>
    public class ConnectingLine : MonoBehaviour
    {
        private Transform TransformA;
        private Transform TransformB;

        private LineRenderer LineRenderer;

        /// <summary>
        /// Create line segment connecting two transforms and attached to a third
        /// </summary>
        /// <param name="parent">Parent to hang the line segment off of</param>
        /// <param name="transformA">Beginning endpoint of line segment</param>
        /// <param name="transformB">Enging endpoint of line segment</param>
        /// <param name="width">Width of the Unity LineRenderer</param>
        /// <param name="color">Color of the line segment</param>
        public static ConnectingLine Create(Transform parent, Transform transformA, Transform transformB, float width, Color color)
        {
            var gameObject = new GameObject("ConnectingLine");
            gameObject.transform.parent = parent; 
            var lineRenderer = gameObject.AddComponent<LineRenderer>();
            Color startColor = color;
            Color endColor = Color.Lerp(color, Color.white - color, 0.5f);
            lineRenderer.startColor = startColor;
            lineRenderer.endColor = endColor;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;

            var lineMaterial = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            lineRenderer.material = lineMaterial;

            var res = gameObject.AddComponent<ConnectingLine>();
            res.LineRenderer = lineRenderer;
            res.TransformA = transformA;
            res.TransformB = transformB;
            return res;
        }

        /// <summary>
        /// Adjust line endpoints to linked transforms
        /// </summary>
        void Update()
        {
            LineRenderer.SetPosition(0, TransformA.position);
            LineRenderer.SetPosition(1, TransformB.position);
        }
    }
}
