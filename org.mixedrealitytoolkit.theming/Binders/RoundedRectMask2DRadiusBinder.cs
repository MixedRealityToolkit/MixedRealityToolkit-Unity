// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using Microsoft.MixedReality.GraphicsTools;
using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    [System.Serializable]
    public class RoundedRectMask2DRadiusBinder : BaseThemeBinder<float, RoundedRectMask2D>
    {
        protected override void Apply(BaseThemeItemData<float> themeItemData)
        {
            if (Target != null)
            {
                Target.Radii = Vector3.one * themeItemData.Value;
            }
        }
    }
}
