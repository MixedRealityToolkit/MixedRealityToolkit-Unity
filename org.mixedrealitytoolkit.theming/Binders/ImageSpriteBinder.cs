// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.UI;

namespace MixedReality.Toolkit.Theming
{
    [System.Serializable]
    public class ImageSpriteBinder : BaseThemeBinder<Sprite, Image>
    {
        protected override void Apply(BaseThemeItemData<Sprite> themeItemData)
        {
            if (Target != null)
            {
                Target.sprite = themeItemData.Value;
            }
        }
    }
}
