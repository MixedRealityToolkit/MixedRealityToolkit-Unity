// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace MixedReality.Toolkit.Theming
{
    [CreateAssetMenu(fileName = "Theme Data Source", menuName = "MRTK/Theming/Theme Data Source", order = 0)]
    public class ThemeDataSource : ScriptableObject, INotifyValueChanged<Theme>
    {
        [SerializeField]
        private Theme activeTheme;

        [SerializeField]
        private UnityEvent<ChangeEvent<Theme>> onThemeChanged = new();

        [SerializeField]
        private ThemeDefinition themeDefinition;

        public ThemeDefinition Definition => themeDefinition;

        #region INotifyValueChanged<Theme>

        /// <inheritdoc/>
        public Theme value
        {
            get => activeTheme;
            set
            {
                if (value != null && value.Definition != null && value.Definition != themeDefinition)
                {
                    Debug.LogError($"New theme's definition ({value.Definition.name}) does not match this data source's definition ({(themeDefinition != null ? themeDefinition.name : "null")})");
                }

                using (ChangeEvent<Theme> changeEvent = ChangeEvent<Theme>.GetPooled(activeTheme, value))
                {
                    // target is intentionally unset, as ThemeDataSource is not a UIElements visual
                    // element, but ChangeEvent<T> is used for its newValue/previousValue semantics
                    // and object pooling
                    SetValueWithoutNotify(value);
                    onThemeChanged.Invoke(changeEvent);
                }
            }
        }

        /// <inheritdoc/>
        public void SetValueWithoutNotify(Theme newValue)
        {
            activeTheme = newValue;
        }

        #endregion INotifyValueChanged<Theme>

        public void AddListener(UnityAction<ChangeEvent<Theme>> action)
        {
            if (action == null) { return; }
            onThemeChanged.AddListener(action);
            if (activeTheme == null) { return; }
            using ChangeEvent<Theme> changeEvent = ChangeEvent<Theme>.GetPooled(null, activeTheme);
            action.Invoke(changeEvent);
        }

        public void RemoveListener(UnityAction<ChangeEvent<Theme>> action)
        {
            if (action == null) { return; }
            onThemeChanged.RemoveListener(action);
        }
    }
}
