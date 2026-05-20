// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.UIElements;

namespace MixedReality.Toolkit.Theming
{
    public abstract class BaseThemeBinder<T, K> : IBinder
    {
        [field: SerializeField]
        protected K Target { get; private set; }

        [field: SerializeField, HideInInspector]
        public string ThemeDefinitionItemName { get; private set; }

        protected abstract void Apply(BaseThemeItemData<T> themeItemData);

        protected void OnThemeChanged(ChangeEvent<Theme> changeEvent)
        {
            if (changeEvent.newValue == null)
            {
                Debug.LogWarning($"{GetType().Name}: Received a theme change event with a null theme. Skipping Apply.");
                return;
            }

            if (!changeEvent.newValue.TryGetItemData(ThemeDefinitionItemName, out BaseThemeItemData<T> value))
            {
                Debug.LogWarning($"{GetType().Name}: No item named '{ThemeDefinitionItemName}' of type '{typeof(T).Name}' found in theme '{changeEvent.newValue.name}'.");
                return;
            }

            Apply(value);
        }

        void IBinder.Subscribe(ThemeDataSource themeDataSource)
        {
            if (themeDataSource != null)
            {
                themeDataSource.AddListener(OnThemeChanged);
            }
        }

        void IBinder.Unsubscribe(ThemeDataSource themeDataSource)
        {
            if (themeDataSource != null)
            {
                themeDataSource.RemoveListener(OnThemeChanged);
            }
        }
    }
}
