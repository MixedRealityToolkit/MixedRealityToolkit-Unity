// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    public class ThemeBinding : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The theme data source manager.")]
        private ThemeDataSource themeDataSource;

        [SerializeReference, InterfaceSelector]
        [Tooltip("The list of bound theme entries.")]
        private IBinder[] binders;

        protected void OnEnable()
        {
            if (themeDataSource == null)
            {
                Debug.LogWarning($"{nameof(ThemeBinding)} on '{gameObject.name}' has no {nameof(ThemeDataSource)} assigned.", this);
                return;
            }

            foreach (IBinder binder in binders)
            {
                if (binder == null)
                {
                    Debug.LogWarning($"{nameof(ThemeBinding)} on '{gameObject.name}' has a null binder entry.", this);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(binder.ThemeDefinitionItemName))
                {
                    Debug.LogWarning($"{nameof(ThemeBinding)} on '{gameObject.name}' has a {binder.GetType().Name} with no theme item assigned.", this);
                }

                binder.Subscribe(themeDataSource);
            }
        }

        protected void OnDisable()
        {
            foreach (IBinder binder in binders)
            {
                binder?.Unsubscribe(themeDataSource);
            }
        }
    }
}
