// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

#if MRTK_THEMING_PRESENT
using MixedReality.Toolkit.Theming;
#endif

using UnityEngine;

namespace MixedReality.Toolkit.UX
{
    public class ThemeBinding : MonoBehaviour
    {
#if MRTK_THEMING_PRESENT
        [SerializeField]
        [Tooltip("The theme data source manager.")]
        private ThemeDataSource themeDataSource;

        [SerializeReference, InterfaceSelector]
        [Tooltip("The list of bound theme entries.")]
        private IBinder[] binders = System.Array.Empty<IBinder>();

        private readonly System.Collections.Generic.List<IBinder> subscribedBinders = new System.Collections.Generic.List<IBinder>();
        private ThemeDataSource subscribedDataSource;

        protected void OnEnable()
        {
            if (themeDataSource == null)
            {
                Debug.LogWarning($"{nameof(ThemeBinding)} on '{gameObject.name}' has no {nameof(ThemeDataSource)} assigned.", this);
                return;
            }

            subscribedDataSource = themeDataSource;

            if (binders != null)
            {
                if (subscribedBinders.Capacity < binders.Length)
                {
                    subscribedBinders.Capacity = binders.Length;
                }

                for (int i = 0; i < binders.Length; i++)
                {
                    IBinder binder = binders[i];
                    if (binder == null)
                    {
                        Debug.LogWarning($"{nameof(ThemeBinding)} on '{gameObject.name}' has a null binder entry.", this);
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(binder.ThemeDefinitionItemName))
                    {
                        Debug.LogWarning($"{nameof(ThemeBinding)} on '{gameObject.name}' has a {binder.GetType().Name} with no theme item assigned.", this);
                    }

                    binder.Subscribe(subscribedDataSource);
                    subscribedBinders.Add(binder);
                }
            }
        }

        protected void OnDisable()
        {
            if (subscribedDataSource != null)
            {
                for (int i = 0; i < subscribedBinders.Count; i++)
                {
                    subscribedBinders[i]?.Unsubscribe(subscribedDataSource);
                }
                subscribedBinders.Clear();
                subscribedDataSource = null;
            }
        }
#endif
    }
}
