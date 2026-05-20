// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    [System.Serializable]
    public class SpriteRendererColorBinder : BaseThemeBinder<Color, SpriteRenderer>
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
