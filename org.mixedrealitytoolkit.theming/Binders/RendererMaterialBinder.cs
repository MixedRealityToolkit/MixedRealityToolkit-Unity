// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    /// <summary>
    /// Binds a material theme data value to the material property of a <see cref="UnityEngine.Renderer"/>.
    /// </summary>
    [System.Serializable]
    public class RendererMaterialBinder : BaseThemeBinder<Material, Renderer>
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
