// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using Microsoft.MixedReality.GraphicsTools;
using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    /// <summary>
    /// Binds a float theme data value to the corner radii property of a <see cref="Microsoft.MixedReality.GraphicsTools.RoundedRectMask2D"/>.
    /// </summary>
    [System.Serializable]
    public class RoundedRectMask2DRadiusBinder : BaseThemeBinder<float, RoundedRectMask2D>
    {
        /// <inheritdoc />
        protected override void Apply(BaseThemeItemData<float> themeItemData)
        {
            if (Target != null)
            {
                Target.Radii = Vector3.one * themeItemData.Value;
            }
        }
    }
}
