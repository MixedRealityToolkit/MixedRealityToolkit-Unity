// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.UI;

namespace MixedReality.Toolkit.Theming
{
    [System.Serializable]
    public class GraphicColorBinder : BaseThemeBinder<Color, Graphic>
    {
        protected override void Apply(BaseThemeItemData<Color> themeItemData)
        {
            if (Target != null)
            {
                Target.color = themeItemData.Value;
            }
        }
    }
}
