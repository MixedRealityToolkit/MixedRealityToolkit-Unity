// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit.UX;
using System;
using TMPro;
using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    [Serializable]
    public class FontIconSetBinder : BaseThemeBinder<FontIconSetBinder.FontIconSetData, FontIconSetBinder.FontIconSetBinding>
    {
        protected override void Apply(BaseThemeItemData<FontIconSetData> themeItemData)
        {
            if (Target != null && Target.TextMeshProComponent != null && Target.IconSelector != null && themeItemData.Value.FontIconSet.GlyphIconsByName.ContainsKey(Target.IconSelector.CurrentIconName))
            {
                // Clear the text to prevent missing character warnings when changing the font
                Target.TextMeshProComponent.text = string.Empty;
                Target.TextMeshProComponent.font = themeItemData.Value.Font;
                Target.IconSelector.SetFontIconSet(themeItemData.Value.FontIconSet, true);
            }
        }

        [Serializable]
        public class FontIconSetBinding
        {
            [field: SerializeField]
            public FontIconSelector IconSelector { get; private set; }

            [field: SerializeField]
            public TMP_Text TextMeshProComponent { get; private set; }
        }

        [Serializable]
        public class FontIconSetData
        {
            [field: SerializeField]
            public TMP_FontAsset Font { get; private set; }

            [field: SerializeField]
            public FontIconSet FontIconSet { get; private set; }
        }
    }
}
