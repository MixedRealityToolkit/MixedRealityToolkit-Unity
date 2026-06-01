// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    [System.Serializable]
    public class SkyboxMaterialBinder : BaseThemeBinder<Material, Skybox>
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
