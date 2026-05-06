// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit.Subsystems;
using System;

namespace MixedReality.Toolkit.Accessibility
{
    /// <summary>
    /// Encapsulates the parameters for creating a new <see cref="AccessibilitySubsystemDescriptor"/>.
    /// </summary>
    public class AccessibilitySubsystemCinfo : MRTKSubsystemCinfo { }

    /// <summary>
    /// Specifies a functionality description that may be registered for each implementation that provides the
    /// <see cref="AccessibilitySubsystem"/> interface.
    /// </summary>
    public class AccessibilitySubsystemDescriptor :
        MRTKSubsystemDescriptor<AccessibilitySubsystem, AccessibilitySubsystem.Provider>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccessibilitySubsystemDescriptor"/> class.
        /// </summary>
        /// <param name="cinfo">The parameters required to initialize the descriptor.</param>
        AccessibilitySubsystemDescriptor(AccessibilitySubsystemCinfo cinfo) : base(cinfo) { }

        /// <summary>
        /// Creates a <see cref="AccessibilitySubsystemDescriptor"/> based on the given parameters.
        /// </summary>
        /// <remarks>
        /// This function will verify that the <see cref="AccessibilitySubsystemCinfo"/> properties are valid.
        /// </remarks>
        /// <param name="cinfo">The parameters required to initialize the descriptor.</param>
        /// <returns>
        /// The newly created instance of the <see cref="AccessibilitySubsystemDescriptor"/> class.
        /// </returns>
        internal static AccessibilitySubsystemDescriptor Create(AccessibilitySubsystemCinfo cinfo)
        {
            // Validates cinfo.
            if (!XRSubsystemHelpers.CheckTypes<AccessibilitySubsystem, AccessibilitySubsystem.Provider>(cinfo.Name,
                                                                                                        cinfo.SubsystemTypeOverride,
                                                                                                        cinfo.ProviderType))
            {
                throw new ArgumentException("Could not create AccessibilitySubsystemDescriptor.");
            }

            return new AccessibilitySubsystemDescriptor(cinfo);
        }
    }
}
