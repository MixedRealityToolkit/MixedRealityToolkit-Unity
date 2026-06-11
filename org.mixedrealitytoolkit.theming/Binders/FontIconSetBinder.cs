// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit.UX;
using System;
using TMPro;
using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    /// <summary>
    /// Binds a font icon set theme data value to a <see cref="FontIconSetBinding"/>.
    /// </summary>
    [Serializable]
    public class FontIconSetBinder : BaseThemeBinder<FontIconSetBinder.FontIconSetData, FontIconSetBinder.FontIconSetBinding>
    {
        /// <inheritdoc />
        protected override void Apply(BaseThemeItemData<FontIconSetData> themeItemData)
        {
            if (Target != null &&
                Target.TextMeshProComponent != null &&
                Target.IconSelector != null &&
                Target.IconSelector.CurrentIconName != null &&
                themeItemData?.Value?.FontIconSet != null &&
                themeItemData.Value.FontIconSet.GlyphIconsByName != null &&
                themeItemData.Value.FontIconSet.GlyphIconsByName.ContainsKey(Target.IconSelector.CurrentIconName))
            {
                // Clear the text to prevent missing character warnings when changing the font
                Target.TextMeshProComponent.text = string.Empty;
                Target.TextMeshProComponent.font = themeItemData.Value.Font;
                Target.IconSelector.SetFontIconSet(themeItemData.Value.FontIconSet, true);
            }
        }

        /// <summary>
        /// A composite binding target for a <see cref="FontIconSelector"/> and a <see cref="TMP_Text"/> component.
        /// </summary>
        [Serializable]
        public class FontIconSetBinding
        {
            /// <summary>
            /// The target icon selector component to apply the font icon set to.
            /// </summary>
            [field: SerializeField]
            public FontIconSelector IconSelector { get; private set; }

            /// <summary>
            /// The target text mesh pro component to apply the font asset to.
            /// </summary>
            [field: SerializeField]
            public TMP_Text TextMeshProComponent { get; private set; }
        }

        /// <summary>
        /// A composite data type containing a <see cref="TMP_FontAsset"/> and a <see cref="MixedReality.Toolkit.UX.FontIconSet"/>.
        /// </summary>
        [Serializable]
        public class FontIconSetData
        {
            /// <summary>
            /// The font asset to apply to the text mesh pro component.
            /// </summary>
            [field: SerializeField]
            public TMP_FontAsset Font { get; private set; }

            /// <summary>
            /// The font icon set to apply to the icon selector component.
            /// </summary>
            [field: SerializeField]
            public FontIconSet FontIconSet { get; private set; }
        }
    }
}
