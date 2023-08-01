// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

namespace MixedReality.Toolkit.Speech.Windows
{
    /// <summary>
    /// The en-US voices that can be used by <see cref="WindowsTextToSpeechSubsystemConfig"/>.
    /// </summary>
    /// <remarks>
    /// Voices for all other locales are categorized as <see cref="Other"/>.
    /// </remarks>
    public enum TextToSpeechVoice
    {
        /// <summary>
        /// The default system voice.
        /// </summary>
        Default,

        /// <summary>
        /// Microsoft David voice
        /// </summary>
        David,

        /// <summary>
        /// Microsoft Mark voice
        /// </summary>
        Mark,

        /// <summary>
        /// Microsoft Zira voice
        /// </summary>
        Zira,

        /// <summary>
        /// Voice not listed above.
        /// </summary>
        /// <remarks>
        /// For use with languages which are not en-US.
        /// </remarks>
        Other
    }
}
