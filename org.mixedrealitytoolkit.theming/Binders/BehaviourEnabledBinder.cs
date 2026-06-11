// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    /// <summary>
    /// Binds a boolean theme data value to the enabled state of a <see cref="UnityEngine.Behaviour"/>.
    /// </summary>
    [System.Serializable]
    public class BehaviourEnabledBinder : BaseThemeBinder<bool, Behaviour>
    {
        /// <inheritdoc />
        protected override void Apply(BaseThemeItemData<bool> themeItemData)
        {
            if (Target != null)
            {
                Target.enabled = themeItemData.Value;
            }
        }
    }
}
