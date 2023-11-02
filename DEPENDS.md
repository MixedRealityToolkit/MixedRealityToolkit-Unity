# Mixed Reality Toolkit for Unity Dependencies

This page catalogs the project's externals dependencies and the reason for requiring a particular dependency version. If an external dependency is added or a dependency version is updated, this page must be updated.

All dependency entries must specify a minimum supported version. If a maximum version is known, it too must be specified. If the maximum version is blank, then there is no known maximum version and versions after the minimum version may work with MRTK. A blank maximum version does not guarantee that all versions after the minimum will function properly.

When updating a dependency version, add a new row to the top of the dependency's history table. The change's date, author, and reason must be specified in the added row.

## Unity

Needed for all MRTK packages.

| Min Version | Max Version | Date          | Author          | Change Reason                        |
|-------------|-------------|---------------|-----------------|--------------------------------------|
| 2021.3.21   |             | Sep. 20, 2023 | @AMollis        | Initial versions                     |

## com.microsoft.mrtk.graphicstools.unity

Required by org.mixedrealitytoolkit.accessibility, org.mixedrealitytoolkit.input, org.mixedrealitytoolkit.standardasset, org.mixedrealitytoolkit.uxcomponents, and org.mixedrealitytoolkit.uxcore.

| Min Version | Max Version | Date          | Author          | Change Reason                        |
|-------------|-------------|---------------|-----------------|--------------------------------------|
| 0.5.12      |             | Sep. 20, 2023 | @AMollis        | Initial version                      |

## com.microsoft.mrtk.tts.windows

Required by org.mixedrealitytoolkit.windowsspeech.

| Min Version | Max Version | Date          | Author          | Change Reason                        |
|-------------|-------------|---------------|-----------------|--------------------------------------|
| 1.0.1       |             | Sep. 20, 2023 | @AMollis        | Initial version                      |

## com.microsoft.spatialaudio.spatializer.unity

Optional dependency for org.mixedrealitytoolkit.audio.

| Min Version | Max Version | Date          | Author          | Change Reason                        |
|-------------|-------------|---------------|-----------------|--------------------------------------|
| 1.0.246     |             | Sep. 20, 2023 | @AMollis        | Initial version                      |

## com.unity.inputsystem

Required by org.mixedrealitytoolkit.input, org.mixedrealitytoolkit.spatialmanipulation, and org.mixedrealitytoolkit.uxcore.

| Min Version | Max Version | Date          | Author          | Change Reason                        |
|-------------|-------------|---------------|-----------------|--------------------------------------|
| 1.6.1       |             | Sep. 20, 2023 | @AMollis        | Initial version                      |

## com.unity.nuget.newtonsoft-json

Required by org.mixedrealitytoolkit.data.

| Min Version | Max Version | Date          | Author          | Change Reason                        |
|-------------|-------------|---------------|-----------------|--------------------------------------|
| 2.0.2       |             | Sep. 20, 2023 | @AMollis        | Initial version                      |

## com.unity.textmeshpro

Required by org.mixedrealitytoolkit.accessibility, org.mixedrealitytoolkit.data, and org.mixedrealitytoolkit.uxcore.

| Min Version | Max Version | Date          | Author          | Change Reason                        |
|-------------|-------------|---------------|-----------------|--------------------------------------|
| 3.0.6       |             | Sep. 20, 2023 | @AMollis        | Initial version                      |

## com.unity.xr.arfoundation

Required by org.mixedrealitytoolkit.input.

| Min Version | Max Version | Date          | Author          | Change Reason                        |
|-------------|-------------|---------------|-----------------|--------------------------------------|
| 5.0.5       |             | Sep. 20, 2023 | @AMollis        | Initial version                      |

## com.unity.xr.core-utils

Required by org.mixedrealitytoolkit.core, and org.mixedrealitytoolkit.input.

| Min Version | Max Version | Date          | Author          | Change Reason                        |
|-------------|-------------|---------------|-----------------|--------------------------------------|
| 2.1.0       |             | Sep. 20, 2023 | @AMollis        | Initial version                      |

## com.unity.xr.hands

Required by org.mixedrealitytoolkit.input.

| Min Version | Max Version | Date          | Author          | Change Reason                           |
|-------------|-------------|---------------|-----------------|-----------------------------------------|
| 1.3.0       |             | Oct. 6, 2023  | @keveleigh      | Added support for Unity's XR Hands API. |

## com.unity.xr.interaction.toolkit

Required by org.mixedrealitytoolkit.core, org.mixedrealitytoolkit.input, org.mixedrealitytoolkit.spatialmanipulation, org.mixedrealitytoolkit.uxcomponents, and org.mixedrealitytoolkit.uxcore.

| Min Version | Max Version | Date          | Author          | Change Reason                        |
|-------------|-------------|---------------|-----------------|--------------------------------------|
| 2.3.0       |             | Sep. 20, 2023 | @AMollis        | Initial version                      |

## com.unity.xr.management

Required by org.mixedrealitytoolkit.core, and org.mixedrealitytoolkit.diagnostics.

| Min Version | Max Version | Date          | Author          | Change Reason                        |
|-------------|-------------|---------------|-----------------|--------------------------------------|
| 4.2.1       |             | Sep. 20, 2023 | @AMollis        | Initial version                      |
