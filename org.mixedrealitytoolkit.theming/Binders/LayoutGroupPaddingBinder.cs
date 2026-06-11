// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.UI;

namespace MixedReality.Toolkit.Theming
{
    /// <summary>
    /// Binds a rect offset theme data value to the padding property of a <see cref="UnityEngine.UI.HorizontalOrVerticalLayoutGroup"/>.
    /// </summary>
    [System.Serializable]
    public class LayoutGroupPaddingBinder : BaseThemeBinder<RectOffset, HorizontalOrVerticalLayoutGroup>
    {
        /// <inheritdoc />
        protected override void Apply(BaseThemeItemData<RectOffset> themeItemData)
        {
            if (Target != null)
            {
                Target.padding = themeItemData.Value;
            }
        }
    }
}
