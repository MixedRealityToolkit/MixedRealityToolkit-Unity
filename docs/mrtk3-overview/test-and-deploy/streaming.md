---
title: Iteration and debugging
description: Iteration and debugging
parent: Test and Experience Overview
---

# Streaming your application to a device

These options detail how to stream your application to the device of your choice. Streaming your application allows for rapid iteration and development, as the application runs locally on your machine, without the need to compile and install onto your device. It also allows you to use Unity's plethora of in-editor debugging tools and features.

- **Recommended:** [Holographic remoting (on HoloLens 2)](/windows/mixed-reality/develop/unity/preview-and-debug-your-app)
  - For development on HoloLens 2 and related platforms (including other OpenXR targets that include hand tracking), we strongly recommend the use of holographic remoting to accelerate your iteration time. Advanced features like hand tracking, eye tracking, and scene reconstruction are available through remoting, and behave the same as if the app were deployed to a device.
- Play-mode testing with the desktop's active OpenXR runtime
  - Many popular PC VR platforms now support OpenXR, including [Windows Mixed Reality](/windows/mixed-reality/develop/native/openxr-getting-started), [SteamVR](https://www.steamvr.com/), and [Oculus Rift on PC](https://developer.oculus.com/documentation/native/pc/dg-openxr/).
- **Experimental**: [Meta Quest Link](https://www.meta.com/quest/)
  - Some aspects of hand interactions are still being developed for Quest, and your results may vary.
  - Controller interactions should be full parity over Link.
  - In **Player Settings** > **OpenXR**, the following must be assigned for the **Windows, Mac, Linux Settings** tab:
    - Set **Play Mode OpenXR Runtime** to **Oculus OpenXR**.
    - Add the **Oculus Touch Controller Profile** to the list of **Interaction Profiles**.
