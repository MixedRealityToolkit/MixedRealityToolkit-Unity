// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.UI;

namespace MixedReality.Toolkit.Theming
{
    /// <summary>
    /// Binds a material theme data value to the material property of a <see cref="UnityEngine.UI.Graphic"/>.
    /// </summary>
    [System.Serializable]
    public class GraphicMaterialBinder : BaseThemeBinder<Material, Graphic>
    {
        /// <inheritdoc />
        protected override void Apply(BaseThemeItemData<Material> themeItemData)
        {
            if (Target != null)
            {
                Target.material = themeItemData.Value;
            }
        }
    }
}
