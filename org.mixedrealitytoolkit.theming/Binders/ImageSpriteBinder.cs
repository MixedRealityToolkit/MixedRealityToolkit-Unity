// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.UI;

namespace MixedReality.Toolkit.Theming
{
    /// <summary>
    /// Binds a sprite theme data value to the sprite property of a <see cref="UnityEngine.UI.Image"/>.
    /// </summary>
    [System.Serializable]
    public class ImageSpriteBinder : BaseThemeBinder<Sprite, Image>
    {
        /// <inheritdoc />
        protected override void Apply(BaseThemeItemData<Sprite> themeItemData)
        {
            if (Target != null)
            {
                Target.sprite = themeItemData.Value;
            }
        }
    }
}
