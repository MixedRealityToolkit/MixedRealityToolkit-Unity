// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.UI;

namespace MixedReality.Toolkit.Theming
{
    [System.Serializable]
    public class GraphicMaterialBinder : BaseThemeBinder<Material, Graphic>
    {
        protected override void Apply(BaseThemeItemData<Material> themeItemData)
        {
            if (Target != null)
            {
                Target.material = themeItemData.Value;
            }
        }
    }
}
