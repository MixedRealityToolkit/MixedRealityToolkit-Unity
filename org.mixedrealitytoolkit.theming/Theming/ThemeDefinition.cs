// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    [Serializable]
    public class ThemeDefinition
    {
        [field: SerializeField]
        [Tooltip("The items defining the theme that can be bound to this source.")]
        public ThemeDefinitionItem[] ThemeDefinitionItems { get; private set; }

        [Serializable]
        public class ThemeDefinitionItem
        {
            [field: SerializeField]
            public string Name { get; set; }

            [field: SerializeField, Extends(typeof(BaseThemeItemData<>), TypeGrouping.ByNamespaceFlat, AllowGenericTypeDefinition = true)]
            public SystemType DataType { get; set; }
        }
    }
}
