// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.UIElements;

namespace MixedReality.Toolkit.Theming
{
    /// <summary>
    /// Base class for a theme binder that binds a theme data value of type <typeparamref name="T"/> to a target component of type <typeparamref name="K"/>.
    /// </summary>
    public abstract class BaseThemeBinder<T, K> : IBinder
    {
        /// <summary>
        /// The target component to apply the theme data to.
        /// </summary>
        [field: SerializeField]
        protected K Target { get; private set; }

        /// <inheritdoc />
        [field: SerializeField, HideInInspector]
        public string ThemeDefinitionItemName { get; private set; }

        /// <summary>
        /// Applies the theme data value to the target component.
        /// </summary>
        /// <param name="themeItemData">The theme data to apply.</param>
        protected abstract void Apply(BaseThemeItemData<T> themeItemData);

        /// <summary>
        /// Invoked when the active theme is changed on the data source.
        /// </summary>
        /// <param name="changeEvent">The change event containing the new theme.</param>
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
