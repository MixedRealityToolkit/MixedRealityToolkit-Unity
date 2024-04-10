using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Tools
{
    /// <summary>
    ///  Very simple class to rotate sprites to face the camera.
    /// </summary>
    public class SimpleSprite : MonoBehaviour
    {
        [Tooltip("Object to face towards. Defaults to camera at time Start is called.")]
        [SerializeField]
        private Transform target = null;

        /// <summary>
        /// Object to face towards. Defaults to camera at time Start is called.
        /// </summary>
        public Transform Target { get { return target; } set { target = value; } }

        [Tooltip("Swivel means rotate about Y-axis, set to false to face directly at target.")]
        [SerializeField]
        private bool swivel = true;

        /// <summary>
        /// Swivel means rotate about Y-axis, set to false to face directly at target.
        /// </summary>
        public bool Swivel { get { return swivel; } set { swivel = value; } }
        // Start is called before the first frame update
        void Start()
        {
            if (target == null)
            {
                target = Camera.main.transform;
            }
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 fromTarget = transform.position - target.position;
            Vector3 targetUp = Vector3.up;
            if (swivel)
            {
                fromTarget.y = 0;
            }
            else if (target.up.sqrMagnitude > 0)
            {
                targetUp = target.up;
            }
            if (fromTarget.sqrMagnitude > 0)
            {
                Quaternion rotation = Quaternion.LookRotation(fromTarget, targetUp);

                transform.rotation = rotation;
            }
        }
    }
}