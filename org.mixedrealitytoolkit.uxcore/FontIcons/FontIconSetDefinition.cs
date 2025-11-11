// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System.Collections.Generic;
using UnityEngine;

namespace MixedReality.Toolkit.UX
{
    [CreateAssetMenu(fileName = "FontIconSetDefinition", menuName = "MRTK/UX/Font Icon Set Definition")]
    public class FontIconSetDefinition : ScriptableObject
    {
        [SerializeField]
        private string[] iconNames;

        public string[] IconNames => iconNames;
    }
}
