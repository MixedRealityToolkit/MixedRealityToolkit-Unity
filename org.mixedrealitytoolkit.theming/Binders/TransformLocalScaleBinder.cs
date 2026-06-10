// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    /// <summary>
    /// Binds a vector3 theme data value to the local scale property of a <see cref="UnityEngine.Transform"/>.
    /// </summary>
    [System.Serializable]
    public class TransformLocalScaleBinder : BaseThemeBinder<Vector3, Transform>
    {
        /// <inheritdoc />
        protected override void Apply(BaseThemeItemData<Vector3> themeItemData)
        {
            if (Target != null)
            {
                Target.localScale = themeItemData.Value;
            }
        }
    }
}
