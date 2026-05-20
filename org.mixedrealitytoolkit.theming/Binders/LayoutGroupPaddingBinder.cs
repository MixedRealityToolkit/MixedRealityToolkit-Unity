// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.UI;

namespace MixedReality.Toolkit.Theming
{
    [System.Serializable]
    public class LayoutGroupPaddingBinder : BaseThemeBinder<RectOffset, HorizontalOrVerticalLayoutGroup>
    {
        protected override void Apply(BaseThemeItemData<RectOffset> themeItemData)
        {
            if (Target != null)
            {
                Target.padding = themeItemData.Value;
            }
        }
    }
}
