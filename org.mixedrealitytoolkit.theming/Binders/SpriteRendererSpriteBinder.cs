// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    [System.Serializable]
    public class SpriteRendererSpriteBinder : BaseThemeBinder<Sprite, SpriteRenderer>
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
