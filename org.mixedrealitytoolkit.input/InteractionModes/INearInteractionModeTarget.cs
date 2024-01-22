// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// This interface is used to update the front plate and rounded rect of a pressable
    /// button if they are flagged as dynamic (based on proximity), it is needed to prevent
    /// a circular reference between MRTK Input and MRTK UX Cosss\re Scripts packages.
    /// </summary>
    public interface INearInteractionModeTarget
    {
        public void UpdateFrontPlateAndRoundedRectIfDynamic(bool enable);
    }
}
