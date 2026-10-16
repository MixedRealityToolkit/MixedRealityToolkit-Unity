# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## Unreleased

### Added

* Added runtime test suite (`SimpleProfilerTests`) verifying null-safety, numeric formatting accuracy, and Zero-GC compliance across Diagnostics profiler and subsystem hot paths. [PR #1149](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1149)

### Fixed

* Fixed `NullReferenceException` in `SimpleProfiler` when any `TMP_Text` serialized fields are left unassigned. [PR #1149](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1149)
* Guarded against division by zero in `MRTKPerformanceStatsSubsystem.UpdateFrameRate` using `MathUtilities.ZeroTolerance` and simplified the frame rate calculation. [PR #1149](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1149)

### Changed

* Eliminated per-frame string allocations in `SimpleProfiler.FixedUpdate` by leveraging `TMP_Text.SetText` native float formatting, reusable `StringBuilder` buffer with zero-allocating integer/fraction formatter, and delta caching to skip updates when stats are unchanged. [PR #1149](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1149)

## [3.0.3] - 2025-11-12

### Fixed

* Updated dependencies to match Unity Asset Store packages. [PR #1054](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1054)
  * org.mixedrealitytoolkit.core 3.2.2

## [3.0.2] - 2024-03-20

### Fixed

* Fixed support for UPM package publishing in the Unity Asset Store. [PR #519](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/519)
