// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    /// <summary>
    /// Binds a sprite theme data value to the sprite property of a <see cref="UnityEngine.SpriteRenderer"/>.
    /// </summary>
    [System.Serializable]
    public class SpriteRendererSpriteBinder : BaseThemeBinder<Sprite, SpriteRenderer>
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
