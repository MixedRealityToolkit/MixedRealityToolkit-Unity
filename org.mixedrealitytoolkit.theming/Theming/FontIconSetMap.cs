// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit.UX;
using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    [CreateAssetMenu(fileName = "MRTK_Theming_FontIconSetMap_New", menuName = "MRTK/Theming/Font Icon Set Map")]
    public class FontIconSetMap : ScriptableObject
    {
        [SerializeField]
        private FontIconSetDefinition setDefinition;

        [SerializeField]
        private FontIconSet[] fontIconSets;
    }
}
