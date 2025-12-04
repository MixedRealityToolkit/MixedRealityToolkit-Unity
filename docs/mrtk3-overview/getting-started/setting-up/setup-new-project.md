---
title: Setting up a new project with MRTK3
parent: Getting started with MRTK3
nav_order: 3
---

# Starting from a new project

Since MRTK3 is a collection of loosely coupled packages, consuming MRTK3 is done differently than the way you consume MRTK 2.x. We don't ship MRTK as a Unity project, so you have to manually add MRTK3 packages to your project in order to consume them.

You're not expected to consume every MRTK package. See [which features are useful to you](../../packages/packages-overview.md) and add only the dependencies that matter.

## Setting up a new Unity project with MRTK3

### 1. Create a new Unity project

Create a new Unity project with Unity 2021.3.21f1 or newer. Close the Unity project before proceeding to the next step.

### 2. Import required dependencies and MRTK3 packages with Mixed Reality Feature Tool

There are a handful of packages that MRTK3 uses that aren't part of this toolkit. To obtain these packages, use the [`Mixed Reality Feature Tool`](https://learn.microsoft.com/windows/mixed-reality/develop/unity/welcome-to-mr-feature-tool) and select the latest versions of the following in the **Discover Features** step.

- **Platform Support → Mixed Reality OpenXR Plugin**
- **Spatial Audio → Microsoft Spatializer** (Optional)

For MRTK3 packages, we highly recommend the following two packages to help you get started quickly:

- **MRTK3 → MRTK Input** (Required for this setup)
- **MRTK3 → MRTK UX Components**

These two packages, along with their dependencies (automatically added by the Feature Tool), will enable you to explore most of our UX offerings and create projects ready to be deployed to various XR devices. You can always come back to the Feature Tool and add more packages to your project later.

Be sure to select the `org.mixedrealitytoolkit.*` packages, and not the deprecated packages. The `com.microsoft.mrtk.*` packages have been deprecated, and are no longer supported.

![Selecting the default MRTK3 packages in Microsoft's Mixed Reality Feature Tool](../../images/mrtk3-featuretool-setup-packages.png)

> [!NOTE]
> For more information on MRTK3 packages, see the [package overview page](../../packages/packages-overview.md).

When you're finished selecting packages, click **Get features**, and then follow the instructions in the Mixed Reality Feature Tool to import the selected packages into your Unity project.

### 3. Open the Unity project

Open the Unity project and wait for Unity to finish importing the newly added packages. There may be two pop-up messages in this process:

1. The first message asks whether you want to enable the new input backend. Select **yes**.
1. The second message asks whether you want to update XR InteractionLayerMask. Select **No Thanks**.

Unity might restart a few times during this process; wait for it to finish before proceeding.

### 4. Configure MRTK profile after import

Once imported, MRTK3 requires a profile to be set for the standalone target platform and each additional target platform.

1. Navigate to **Edit > Project Settings**.
1. Under **Project Settings**, navigate to **MRTK3** and then switch to the standalone tab. Note that the profile is initially unspecified.
1. Populate the field with the default MRTK profile that ships with the core package. You can click the "Assign MRTK Default" button to auto-populate this field. Alternatively, you can find the profile under `Packages/org.mixedrealitytoolkit.core/Configuration/Default Profiles/MRTKProfile.asset`.

    > [!NOTE]
    > Not all of the MRTK subsystems are shown in the screenshot below. The MRTK subsystems that you see may be different depending on the MRTK3 packages you've added to your project.

    ![Assign the default MRTK profile](../../images/mrtk-profile.png)

1. Switch to the tabs of other build target(s) you want to use (for example, UWP, Android) and check to see if the profile is assigned. If not, repeat the previous step on the current tab.

### 5. Configure OpenXR-related settings

Once imported, MRTK3 requires some configuration on OpenXR if you're targeting a specific XR device. Refer to the instructions on the following pages for platform-specific guidance.

- [Deploy to an Android XR device](../../test-and-deploy/android-xr-deployment.md)
- [Deploy to a Quest device](../../test-and-deploy/quest-deployment.md)
- [Deploy to HoloLens 2](../../test-and-deploy/hololens2-deployment.md)

### 6. Congratulations, the project setup is now finished

Proceed to [creating a new MRTK3 scene](./setup-new-scene.md).

## Next steps

Once you've finished setting up your Unity project, learn how to [experience your application on a device](../../test-and-deploy/overview.md)
