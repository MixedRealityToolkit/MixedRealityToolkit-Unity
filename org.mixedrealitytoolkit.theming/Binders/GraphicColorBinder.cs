// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.UI;

namespace MixedReality.Toolkit.Theming
{
    /// <summary>
    /// Binds a color theme data value to the color property of a <see cref="UnityEngine.UI.Graphic"/>.
    /// </summary>
    [System.Serializable]
    public class GraphicColorBinder : BaseThemeBinder<Color, Graphic>
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
