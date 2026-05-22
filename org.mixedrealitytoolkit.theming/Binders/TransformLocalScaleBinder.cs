// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    [System.Serializable]
    public class TransformLocalScaleBinder : BaseThemeBinder<Vector3, Transform>
    {
        protected override void Apply(BaseThemeItemData<Vector3> themeItemData)
        {
            if (Target != null)
            {
                Target.localScale = themeItemData.Value;
            }
        }
    }
}
