// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.SpatialManipulation
{
    /// <summary>
    /// Positions a game object with a RadialView component at the center of the view,
    /// maintaining a distance equal to the average of the MinDistance and MaxDistance
    /// properties of the RadialView. This prevents the object from rapidly approaching the
    /// viewer and ensures it appears upright.
    /// </summary>
    [RequireComponent(typeof(RadialView))]
    public class RadialViewDisplayInitializeHelper : MonoBehaviour
    {
        private RadialView radialView;

        protected void Awake()
        {
            radialView = GetComponent<RadialView>();
        }

        private void OnEnable()
        {
            var distance = (radialView.MinDistance + radialView.MaxDistance) / 2.0f;
            transform.position = Camera.main.transform.position +
                                 Camera.main.transform.forward.normalized * distance;
        }
    }
}
