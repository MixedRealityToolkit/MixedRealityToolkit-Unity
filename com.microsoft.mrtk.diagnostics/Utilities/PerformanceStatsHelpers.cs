// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

namespace MixedReality.Toolkit.Diagnostics
{
    /// <summary>
    /// Helper methods for working with Performance Stats components.
    /// </summary>
    public static class PerformanceStatsHelpers
    {
        private static PerformanceStatsSubsystem subsystem = null;

        /// <summary>
        /// The first running PerformanceStatsSubsystem instance    .
        /// </summary>
        public static PerformanceStatsSubsystem Subsystem
        {
            get
            {
                if (subsystem == null || !subsystem.running)
                {
                    subsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<PerformanceStatsSubsystem>();
                }
                return subsystem;
            }
        }
    }
}
