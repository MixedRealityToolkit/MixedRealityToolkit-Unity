# Mixed Reality Toolkit for Unity

![Mixed Reality Toolkit](./images/MRTK_Unity_header.png)

![MRTK3 Banner](./images/MRTK3_banner.png)

**MRTK3** is the third generation of the Mixed Reality Toolkit for Unity. It's an open source project designed to accelerate cross-platform mixed reality development in Unity. MRTK3 is built on top of [Unity's XR Interaction Toolkit (XRI)](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@2.1/manual/index.html) and OpenXR. This new generation of MRTK is intended to be faster, cleaner, and more modular, with an easier cross-platform development workflow enabled by OpenXR and the Unity Input System.

[Mixed Reality Toolkit Organization](https://github.com/MixedRealityToolkit) maintains MRTK3, released MRTK3 for general availability (GA), and will announce future releases.

## Key improvements

### Architecture

* Built on Unity XR Interaction Toolkit and the Unity Input System.
* Dedicated to OpenXR, with flexibility for other XRSDK backends.
* Open-ended and extensible interaction paradigms across devices, platforms, and applications.

### Performance

* Rewrote and redesigned most features and systems, from UX to input to subsystems.
* Zero per-frame memory allocation.
* Tuned for maximum performance on mixed reality devices and other resource-constrained mobile platforms.

### User Interface

* New interaction models (gaze-pinch indirect manipulation).
* Updated Mixed Reality Design Language.
* Unity Canvas + 3D UX: production-grade dynamic auto-layout.
* Unified 2D & 3D input for gamepad, mouse, and accessibility support.
* Data binding for branding, theming, dynamic data, and complex lists.

### Long Term Support

* Minimum requirements: OpenXR, Unity 2022.3 LTS, Unity’s XR Interaction Toolkit 3.0.

## Packages

| Name | Package | Changelog |
|------|---------|-----------|
| Audio Effects | [org.mixedrealitytoolkit.audio](./org.mixedrealitytoolkit.audio) | [Changelog](./org.mixedrealitytoolkit.audio/CHANGELOG.md) |
| Core Definitions | [org.mixedrealitytoolkit.core](./org.mixedrealitytoolkit.core) | [Changelog](./org.mixedrealitytoolkit.core/CHANGELOG.md) |
| Diagnostics | [org.mixedrealitytoolkit.diagnostics](./org.mixedrealitytoolkit.diagnostics) | [Changelog](./org.mixedrealitytoolkit.diagnostics/CHANGELOG.md) |
| Extended Assets | [org.mixedrealitytoolkit.extendedassets](./org.mixedrealitytoolkit.extendedassets) | [Changelog](./org.mixedrealitytoolkit.extendedassets/CHANGELOG.md) |
| Input | [org.mixedrealitytoolkit.input](./org.mixedrealitytoolkit.input) | [Changelog](./org.mixedrealitytoolkit.input/CHANGELOG.md) |
| Spatial Manipulation | [org.mixedrealitytoolkit.spatialmanipulation](./org.mixedrealitytoolkit.spatialmanipulation) | [Changelog](./org.mixedrealitytoolkit.spatialmanipulation/CHANGELOG.md) |
| Standard Assets | [org.mixedrealitytoolkit.standardassets](./org.mixedrealitytoolkit.standardassets) | [Changelog](./org.mixedrealitytoolkit.standardassets/CHANGELOG.md) |
| Tools | [org.mixedrealitytoolkit.tools](./org.mixedrealitytoolkit.tools) | [Changelog](./org.mixedrealitytoolkit.tools/CHANGELOG.md) |
| UX Components | [org.mixedrealitytoolkit.uxcomponents](./org.mixedrealitytoolkit.uxcomponents) | [Changelog](./org.mixedrealitytoolkit.uxcomponents/CHANGELOG.md) |
| UX Components (Non-Canvas) | [org.mixedrealitytoolkit.uxcomponents.noncanvas](./org.mixedrealitytoolkit.uxcomponents.noncanvas) | [Changelog](./org.mixedrealitytoolkit.uxcomponents.noncanvas/CHANGELOG.md) |
| Windows Speech | [org.mixedrealitytoolkit.windowsspeech](./org.mixedrealitytoolkit.windowsspeech) | [Changelog](./org.mixedrealitytoolkit.windowsspeech/CHANGELOG.md) |

### Early preview packages

Some parts of MRTK3 are at earlier stages of the development process than others. Early preview packages can be identified in the Mixed Reality Feature Tool and Unity Package Manager by the `Early Preview` designation in their names.

The following components are considered to be in early preview.

| Name | Package | Changelog |
|------|---------|-----------|
| Accessibility | [org.mixedrealitytoolkit.accessibility](./org.mixedrealitytoolkit.accessibility) | [Changelog](./org.mixedrealitytoolkit.accessibility/CHANGELOG.md) |

It is important to note that the packages may not contain the complete feature set that is planned to be released or they may undergo major, breaking architectural changes before release.

We encourage you to provide any and all feedback to help shape the final form of these early preview features.

## Getting started

[Follow the documentation for setting up MRTK3 packages as dependencies in your project here.](./docs/mrtk3-overview/getting-started/setting-up/setup-new-project.md) Alternatively, you can clone this repo directly to experiment with our template project. However, we *strongly* recommend adding MRTK3 packages as dependencies through the Feature Tool, as it makes updating, managing, and consuming MRTK3 packages far easier and less error-prone.

## Supported devices

| Platform | Supported Devices |
|----------|-------------------|
| OpenXR devices | [Android XR](https://www.android.com/xr/) <br> Microsoft HoloLens 2 <br> Meta Quest <br> Magic Leap 2 <br> Lenovo ThinkReality A3 (with [Qualcomm Snapdragon Spaces](https://docs.spaces.qualcomm.com/unity/samples/preview/mrtk3-setup-guide)) <br> Windows Mixed Reality (experimental) <br> SteamVR (experimental) <br> Varjo XR-3 (experimental) <br> **If your OpenXR device already works with MRTK3, let us know!** |
| Windows | Traditional flat-screen desktop (experimental) |

## UX building blocks

|   |   |   |
|---|---|---|
| ![Button](./docs/mrtk3-overview/images/UXBuildingBlocks/MRTK_UX_v3_Button.png) <br> **Button** <br> A volumetric button optimized for a wide range of input modalities, including poking, gaze-pinch, ray interactions, mouse click, and gamepad. | ![Bounds Control](./docs/mrtk3-overview/images/UXBuildingBlocks/MRTK_UX_v3_BoundsControl.png) <br> **Bounds Control** <br> Intent feedback and precision manipulation affordances. | ![Object Manipulator](./docs/mrtk3-overview/images/UXBuildingBlocks/MRTK_UX_v3_ObjectManipulator.png) <br> **Object Manipulator** <br> Move and manipulate objects with one or two hands with a wide variety of input modalities. |
| ![Hand Menu](./docs/mrtk3-overview/images/UXBuildingBlocks/MRTK_UX_v3_HandMenu.png) <br> **Hand Menu** <br> A hand-anchored collection of UX controls for easy access to quick actions. | ![Near Menu](./docs/mrtk3-overview/images/UXBuildingBlocks/MRTK_UX_v3_NearMenu.png) <br> **Near Menu** <br> Collection of UX controls that can be manipulated, pinned, and set to follow the user. | ![Slider](./docs/mrtk3-overview/images/UXBuildingBlocks/MRTK_UX_v3_Slider.png) <br> **Slider** <br> Adjust a value along a one-dimensional axis. |
| ![Solver](./docs/mrtk3-overview/images/UXBuildingBlocks/MRTK_Solver_Main.png) <br> **Solver** <br> Various object positioning behaviors such as tag-along, body-lock, constant view size and surface magnetism. | ![Dialog](./docs/mrtk3-overview/images/UXBuildingBlocks/MRTK_UX_v3_Dialog.png) <br> **Dialog** <br> Prompt for user action. | ![Slate](./docs/mrtk3-overview/images/UXBuildingBlocks/MRTK_UX_v3_Slate.png) <br> **Slate** <br> A flat panel for displaying large-format interfaces and content. |

### Figma Toolkit for MRTK3 Preview

The [prerelease of Figma Toolkit for MRTK3](https://www.figma.com/community/file/1145959192595816999) includes UI components based on Microsoft's new Mixed Reality Design Language introduced in MRTK3. You can use the 2D representations of the components in the design process for creating UI layouts and storyboards.

## Session videos from Microsoft Mixed Reality Dev Days 2022

|   |   |   |
|---|---|---|
| [![Introducing MRTK3](./docs/mrtk3-overview/images/MRDevDays/MRDD-June8-04-IntroducingMRTK3-1920x1080_w800.png)](https://youtu.be/fjQFkeF-ZOM?list=PLlrxD0HtieHhkPlibqfQf1pGvM0vLNPpL) <br> **[Introducing MRTK3 – Shaping the future of the MR Developer Experience](https://youtu.be/fjQFkeF-ZOM?list=PLlrxD0HtieHhkPlibqfQf1pGvM0vLNPpL)** | [![Getting started with your first MRTK3 project](./docs/mrtk3-overview/images/MRDevDays/MRDD-04-GettingStartedMRTK3-1920x1080_w800.png)](https://youtu.be/aVnwIq4VUcY?list=PLlrxD0HtieHhkPlibqfQf1pGvM0vLNPpL) <br> **[Getting started with your first MRTK3 project](https://youtu.be/aVnwIq4VUcY?list=PLlrxD0HtieHhkPlibqfQf1pGvM0vLNPpL)** | [![MRTK3 Interaction building blocks](./docs/mrtk3-overview/images/MRDevDays/MRDD-07-MRTK3BuildingBlocks-1920x1080_w800.png)](https://youtu.be/naVziEJ-yDg?list=PLlrxD0HtieHhkPlibqfQf1pGvM0vLNPpL) <br> **[MRTK3 Interaction building blocks](https://youtu.be/naVziEJ-yDg?list=PLlrxD0HtieHhkPlibqfQf1pGvM0vLNPpL)** |
| [![Building Rich UI for MR in MRTK3](./docs/mrtk3-overview/images/MRDevDays/MRDD-10-BuildingRichUI-1920x1080_w800.png)](https://youtu.be/g2HF5HMy-2c?list=PLlrxD0HtieHhkPlibqfQf1pGvM0vLNPpL) <br> **[Building Rich UI for MR in MRTK3](https://youtu.be/g2HF5HMy-2c?list=PLlrxD0HtieHhkPlibqfQf1pGvM0vLNPpL)** | [![Working with Dynamic Data and Theming in MRTK3](./docs/mrtk3-overview/images/MRDevDays/MRDD-12-WorkingWithDynamicData-1920x1080_w800.png)](https://youtu.be/IiTpZ2ojyno?list=PLlrxD0HtieHhkPlibqfQf1pGvM0vLNPpL) <br> **[Working with Dynamic Data and Theming in MRTK3](https://youtu.be/IiTpZ2ojyno?list=PLlrxD0HtieHhkPlibqfQf1pGvM0vLNPpL)** | [![#Open - Deploy Everywhere with OpenXR and MRTK3](./docs/mrtk3-overview/images/MRDevDays/MRDD-15-HashOpenDeploy-1920x1080_w800.png)](https://youtu.be/LI6lyW9TG9o?list=PLlrxD0HtieHhkPlibqfQf1pGvM0vLNPpL) <br> **[#Open - Deploy Everywhere with OpenXR and MRTK3](https://youtu.be/LI6lyW9TG9o?list=PLlrxD0HtieHhkPlibqfQf1pGvM0vLNPpL)** |

### Versioning

In previous versions of MRTK (HoloToolkit and MRTK v2), all packages were released as a complete set, marked with the same version number (ex: 2.8.0). Starting with MRTK3 GA, each package will be individually versioned, following the [Semantic Versioning 2.0.0 specification](https://semver.org/spec/v2.0.0.html). (As a result, the '3' in MRTK3 is not a version number!)

Individual versioning will enable faster servicing while providing improved developer understanding of the magnitude of changes and reducing the number of packages needing to be updated to acquire the desired fix(es).

For example, if a non-breaking new feature is added to the UX core package, which contains the logic for user interface behavior the minor version number will increase (from 3.0.x to 3.1.0). Since the change is non-breaking, the UX components package, which depends upon UX core, is not required to be updated.

As a result of this change, there is not a unified MRTK3 product version.

To help identify specific packages and their versions, MRTK3 provides an about dialog that lists the relevant packages included in the project. To access this dialog, select `Mixed Reality` > `MRTK3` > `About MRTK` from the Unity Editor menu.

![About MRTK Panel](images/AboutMRTK.png)

## Branch Status

[Mixed Reality Toolkit Organization](https://github.com/MixedRealityToolkit) maintains and updates MRTK3. We appreciate your feedback, and you can open bugs and feature request at the [Mixed Reality Toolkit for Unity](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity) GitHub project.

## Contributing

This project welcomes contributions, suggestions, and feedback. All contributions, suggestions, and feedback you submitted are accepted under the [Project's license](./LICENSE.md). You represent that if you do not own copyright in the code that you have the authority to submit it under the [Project's license](./LICENSE.md). All feedback, suggestions, or contributions are not confidential.

For more information on how to contribute Mixed Reality Toolkit for Unity Project, please read the [contributing guidelines](./CONTRIBUTING.md).

## Governance

For information on how the Mixed Reality Toolkit for Unity Project is governed, please read the [Governance document](./GOVERNANCE.md).

All projects under the Mixed Reality Toolkit organization are governed by the Steering Committee. The Steering Committee is responsible for all technical oversight, project approval and oversight, policy oversight, and trademark management for the Organization. To learn more about the Steering Committee, visit this link: <https://github.com/MixedRealityToolkit/MixedRealityToolkit-MVG/blob/main/org-docs/CHARTER.md>
