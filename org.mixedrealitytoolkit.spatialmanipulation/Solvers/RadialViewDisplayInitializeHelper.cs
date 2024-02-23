using UnityEngine;

namespace MixedReality.Toolkit.SpatialManipulation
{
    /// <summary>
    /// Makes a game object appear having a RadialView component in the center of the view,
    /// at a distance equal to the average of the MinDistance and MaxDistance of the RadialView.
    /// This prevents the object from 'racing into your view' and/or appear slanted
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
