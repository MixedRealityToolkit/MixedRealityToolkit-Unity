// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using TMPro;

namespace MixedReality.Toolkit.Theming
{
    /// <summary>
    /// Binds a font asset theme data value to the font property of a <see cref="TMPro.TMP_Text"/>.
    /// </summary>
    [System.Serializable]
    public class TMP_TextFontAssetBinder : BaseThemeBinder<TMP_FontAsset, TMP_Text>
    {
        /// <inheritdoc />
        protected override void Apply(BaseThemeItemData<TMP_FontAsset> themeItemData)
        {
            if (Target != null)
            {
                Target.font = themeItemData.Value;
            }
        }
    }
}
