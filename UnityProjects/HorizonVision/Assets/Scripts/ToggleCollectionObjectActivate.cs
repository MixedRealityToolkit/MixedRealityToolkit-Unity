// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for samples. While nice to have, this XML documentation is not required for samples.
#pragma warning disable CS1591

using MixedReality.Toolkit.UX;
using UnityEngine;

namespace MixedReality.Toolkit.Examples.Demos
{
    [AddComponentMenu("MRTK/Examples/Toggle Collection Object Activate")]
    public class ToggleCollectionObjectActivate : MonoBehaviour
    {
        [Tooltip("The ToggleCollection to listen to.")]
        [SerializeField]
        private ToggleCollection toggleCollection;

        [Tooltip("Array of Objects to be controlled by the toggle collection")]
        [SerializeField]
        private GameObject[] targetObjects;

        /// <summary>
        /// A Unity event function that is called on the frame when a script is enabled just before any of the update methods are called the first time.
        /// </summary> 
        private void Start()
        {
            Set(toggleCollection.CurrentIndex);
            toggleCollection.OnToggleSelected.AddListener((toggleSelectedIndex) => Set(toggleSelectedIndex));
        }

        private void Set(int index)
        {
            for (int i = 0; i < targetObjects.Length; i++)
            {
                targetObjects[i].SetActive(i == index);
            }
        }
    }
}
#pragma warning restore CS1591