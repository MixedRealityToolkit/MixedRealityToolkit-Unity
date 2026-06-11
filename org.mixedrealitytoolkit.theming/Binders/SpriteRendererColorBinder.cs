// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    /// <summary>
    /// Binds a color theme data value to the color property of a <see cref="UnityEngine.SpriteRenderer"/>.
    /// </summary>
    [System.Serializable]
    public class SpriteRendererColorBinder : BaseThemeBinder<Color, SpriteRenderer>
    {
        /// <inheritdoc />
        protected override void Apply(BaseThemeItemData<Color> themeItemData)
        {
            if (Target != null)
            {
                Target.color = themeItemData.Value;
            }
        }
    }
}
