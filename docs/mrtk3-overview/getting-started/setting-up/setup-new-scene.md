---
title: Creating a new scene with MRTK3
parent: Getting started with MRTK3
nav_order: 4
---

# Creating a new scene with MRTK3

The following will walk through through creating an AR/VR ready scene using MRTK3.

1. Create a new Unity scene.
1. Add the **MRTK XR Rig** prefab.
1. Remove the **Main Camera** GameObject because **MRTK XR Rig** already contains a camera.

   ![MRTK XR rig screenshot](../../images/mrtk-xr-rig.png)

1. Add the MRTK Input Simulator prefab to your scene.

    > [!NOTE]
    > This step is optional, but required for in-editor input simulation.

    ![MRTK input simulator hierarchy pane](../../images/mrtk-input-simulator.png)

## Next steps

Once you've finished setting up your Unity project, learn how to [experience your application on a device](../../test-and-deploy/overview.md).
